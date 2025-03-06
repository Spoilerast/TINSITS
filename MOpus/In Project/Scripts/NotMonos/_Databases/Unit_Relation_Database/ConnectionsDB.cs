using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using NotMonos.Processors;

namespace NotMonos.Databases
{
	internal sealed partial class ConnectionsDB
	{
		private readonly Dictionary<UnitId, ushort> _connectedUnits = new(50);
		private readonly List<Link> _links = new(100);
		private readonly Dictionary<ushort, Net> _nets = new(20);
		private readonly HashSet<ushort> _netsToIntegralityCheck = new(6);
		private readonly Stack<ushort> _removedNetIds = new(6);
		private readonly Stack<UnitId> _unitsToReconnection = new(6);

		internal void Clear()
		{
			_connectedUnits.Clear();
			_links.Clear();
			_nets.Clear();
			_removedNetIds.Clear();
		}

		internal void Debug_PrintNets(string mes = null) //todo remove
		{
			if (mes != null)
				PeekLogger.Log(mes);

			PeekLogger.LogItems(_connectedUnits);
			foreach (var net in _nets)
			{
				PeekLogger.LogMessage("net id " + net.Key);
				PeekLogger.LogItems(net.Value.UnitsIds);
			}
		}

		internal void DropClusterConnections(UnitId[] disconnected)

		{
			PeekLogger.LogName();
			if (_connectedUnits.TryGetValue(disconnected[0], out var netId))
			{
				_nets[netId].RemoveUnits(disconnected);
				_ = _netsToIntegralityCheck.Add(netId);
			}
			else
				throw new InvalidOperationException("Cluster was not connected");

			disconnected.ForEach(x => _connectedUnits.Remove(x));
		}

		internal void DropConnections(UnitId unitId)
		{
			if (!_connectedUnits.ContainsKey(unitId))
				return;

			ushort netId = _connectedUnits[unitId];
			_nets[netId].RemoveUnit(unitId);
			_ = _netsToIntegralityCheck.Add(netId);
			_ = _connectedUnits.Remove(unitId);
		}

		internal IEnumerable<(bool, IEnumerable<(GridPoint, GridPoint)>)> GetAllNetsLinks()
					=> from net in _nets
					   select (IsNetPowerSourced(net.Key), net.Value.LinksPoints);

		internal void MakeConnections(UnitId unitId, ClusterStatus status)
		{
			if (status is ClusterStatus.NotClustered)
				SingleConnections(unitId); //single unit connections
			else if (status is ClusterStatus.Clustered)
				ClusterConnections(unitId);
			else
				SuperClusterConnections(unitId);
		}

		internal void NeighborizeCluster(IEnumerable<UnitId> clusterUnitIds)
		{
			GetNetAndUnitsPoints(clusterUnitIds, out var netId, out var unitsPoints);
			CreateLinksInCircle(netId, unitsPoints);
			MakeConnections(clusterUnitIds.First(), ClusterStatus.Clustered);
		}

		internal void NeighborizeSuperCluster(IEnumerable<UnitId> clusterUnitIds)
		{
			UnitId axisId = clusterUnitIds.Last();
			DropConnections(axisId);

			GetNetAndUnitsPoints(clusterUnitIds, out var netId, out var unitsPoints);
			CreateLinksInCircle(netId, unitsPoints.SkipLast(1));

			CreateLinksForSuperCluster(axisId, netId, unitsPoints);

			MakeConnections(axisId, ClusterStatus.SuperClustered);
		}

		internal void RefreshConnections()
		{
			ReconnectAll();
			NetsIntegralityCheck();
		}

		internal void RemakeConnections(UnitId[] clusterUnits, ClusterStatus status)//todo unfinished
		{
			//var net = _connectedUnits[clusterUnits[0]];
			DropClusterConnections(clusterUnits);
			//_nets[net].IntegralityCheck(); //where endless loop?
			if (status is ClusterStatus.Clustered)			
				NeighborizeCluster(clusterUnits);
			else
				NeighborizeSuperCluster(clusterUnits);
		}

		internal void RemoveConnections(UnitId unitId)
		{
			DropConnections(unitId);
			ToReconnection(unitId);
		}

		internal bool TryGetNeighborsTo(UnitId unitId, out UnitId[] neighbors)
		{
			neighbors = null;
			if (!_connectedUnits.ContainsKey(unitId))
				return false;

			ushort netId = _connectedUnits[unitId];
			_nets[netId].GetNeighborsTo(unitId, out neighbors);
			return true;
		}

		private void AddOrUpdateConnectedUnit(UnitId id, ushort netId)
		{
			if (id.IsPowerSource)
				return;
			//PeekLogger.LogItems(_connectedUnits);
			//PeekLogger.LogTabTab($"add or update {id} {netId}");
			if (_connectedUnits.ContainsKey(id))
			{
				_connectedUnits[id] = netId;
				//PeekLogger.LogItems(_connectedUnits);
				return;
			}
			_connectedUnits.Add(id, netId);
			//PeekLogger.LogItems(_connectedUnits);
		}

		private void ClusterConnections(UnitId unitId)
		{
			PeekLogger.LogName();
			IEnumerable<(UnitId, GridPoint, UnitId, GridPoint)> allConnections =
				ConnectionProcessor.GetAllClusterConnections(unitId);
			foreach (var (idA, positionA, idB, positionB) in allConnections)
			{
				PeekLogger.LogItemsVarious(idA, idB);
				MakeConnection(idA, positionA, idB, positionB);
			}
		}

		private void ConnectOneWithMany(UnitId unitId, GridPoint unitPosition, ref IEnumerable<(UnitId, GridPoint)> connections)
		{
			foreach (var (neighborId, neighborPosition) in connections)
				MakeConnection(unitId, unitPosition, neighborId, neighborPosition);
		}

		private bool ContainsLink(GridPoint pointA, GridPoint pointB)
			=> _links.Any(x => x.IsConnectionEqual(pointA, pointB));

		private void CreateLink(UnitId idA, GridPoint positionA, UnitId idB, GridPoint positionB, ref ushort netId)
		{
			if (ContainsLink(positionB, positionB))
				return;

			Link link = new(idA, positionA, idB, positionB);
			_links.Add(link);
			//PeekLogger.LogWarning($"Created link {link} for net {netId}");
			_nets[netId].AddLink(link);
		}

		private void CreateLinksForSuperCluster(UnitId axisId, ushort netId, IEnumerable<(UnitId, GridPoint)> unitsPoints)
		{
			GridPoint pointOfAxis = DataCenter.Grid.GetPoint(axisId);
			int[] with = Constants.IsTopOrientation(pointOfAxis)
				? (new[] { 0, 2, 4 })
				: (new[] { 1, 3, 5 });

			MakeLink(6, with[0]);
			MakeLink(6, with[1]);
			MakeLink(6, with[2]);

			void MakeLink(int indexA, int indexB)
			{
				UnitId idA = unitsPoints.ElementAt(indexA).Item1,
						idB = unitsPoints.ElementAt(indexB).Item1;
				GridPoint positionA = unitsPoints.ElementAt(indexA).Item2,
							positionB = unitsPoints.ElementAt(indexB).Item2;
				CreateLink(idA, positionA, idB, positionB, ref netId);
			}
		}

		private void CreateLinksInCircle(ushort netId, IEnumerable<(UnitId, GridPoint)> unitsPoints)
		{
			int count = unitsPoints.Count(),
				end = count - 1;
			for (int i = 0; i < end; i++)
			{
				MakeLink(i, i + 1);
			}
			MakeLink(end, 0);

			void MakeLink(int indexA, int indexB)
			{
				UnitId idA = unitsPoints.ElementAt(indexA).Item1,
						idB = unitsPoints.ElementAt(indexB).Item1;
				GridPoint positionA = unitsPoints.ElementAt(indexA).Item2,
							positionB = unitsPoints.ElementAt(indexB).Item2;
				CreateLink(idA, positionA, idB, positionB, ref netId);
			}
		}

		private void CreateNewNet(UnitId unitIdA, GridPoint unitPositionA, UnitId unitIdB, GridPoint unitPositionB)
		{
			ushort netId = NewNet();
			_nets[netId].AddUnits(unitIdA, unitIdB);
			CreateLink(unitIdA, unitPositionA, unitIdB, unitPositionB, ref netId);
		}

		private void CreateNewNetsEvent(IEnumerable<Net> nets)
		{
			foreach (Net net in nets)
			{
				_ = NewNet(net);
				UpdateConnectedUnitsList(net);
			}
		}

		private void DestroyNet(ushort id)
		{
			_nets[id].UnitsIds.ForEach(uid => _connectedUnits.Remove(uid));
			_ = _nets.Remove(id);
			_removedNetIds.Push(id);
		}

		private void EnterInNet(UnitId netOwnerId, GridPoint ownerPosition, UnitId unitId, GridPoint unitPosition)
		{
			ushort netId = _connectedUnits[netOwnerId];
			_nets[netId].AddUnits(unitId);
			CreateLink(netOwnerId, ownerPosition, unitId, unitPosition, ref netId);
		}

		private void GetNetAndUnitsPoints(
			IEnumerable<UnitId> clusterUnitIds,
			out ushort netId,
			out IEnumerable<(UnitId, GridPoint)> unitsPoints)
		{
			GridDB grid = DataCenter.Grid;
			netId = NewNet();
			unitsPoints =
				from uid in clusterUnitIds
				select (uid, grid.GetPoint(uid));
			_nets[netId].AddUnits(clusterUnitIds.ToArray());
		}

		private ushort GetNetId()
		{
			ushort value;
			if (_removedNetIds.Count == 0)
			{
				value = _nets.LastOrDefault().Key;
				value++;
			}
			else
				value = _removedNetIds.Pop();

			return value;
		}

		private bool IsNetPowerSourced(ushort netId)
			=> _nets[netId].HaveUnit(UnitId.PowerSource);

		private bool IsUnitInNet(UnitId unitId)
			=> _connectedUnits.ContainsKey(unitId);

		private void MakeConnection(
			UnitId initiatorId,
			GridPoint initiatorPosition,
			UnitId neighborId,
			GridPoint neighborPosition)
		{
			bool gridHaveThisLink = ContainsLink(initiatorPosition, neighborPosition);
			//PeekLogger.LogName(gridHaveThisLink);
			if (gridHaveThisLink)
				return;

			bool initiatorHaveNet = IsUnitInNet(initiatorId),
				neighborHaveNet = !neighborId.IsPowerSource && IsUnitInNet(neighborId);
			//PeekLogger.LogItemsVarious(initiatorId, neighborId, initiatorHaveNet, neighborHaveNet);
			if (!neighborHaveNet & !initiatorHaveNet) //both out of nets
				CreateNewNet(initiatorId, initiatorPosition, neighborId, neighborPosition);
			else if (neighborHaveNet && !initiatorHaveNet)
				EnterInNet(neighborId, neighborPosition, initiatorId, initiatorPosition);
			else if (!neighborHaveNet & initiatorHaveNet) //yes, one & is intentional
				EnterInNet(initiatorId, initiatorPosition, neighborId, neighborPosition);
			else if (_connectedUnits[initiatorId] == _connectedUnits[neighborId]) //both in same net
				EnterInNet(neighborId, neighborPosition, initiatorId, initiatorPosition);
			else //both have their own nets
				MergeNets(neighborId, neighborPosition, initiatorId, initiatorPosition);
		}

		private void MergeNets(UnitId netOwnerId, GridPoint ownerPosition, UnitId unitId, GridPoint unitPosition)
		{
			ushort netA, netB, shorter, longer;
			netA = _connectedUnits[netOwnerId];
			netB = _connectedUnits[unitId];

			ShorterNet(netA, netB, out shorter, out longer);
			Net shorterNet = _nets[shorter];
			DestroyNet(shorter);
			_nets[longer].MergeWith(shorterNet);
			CreateLink(netOwnerId, ownerPosition, unitId, unitPosition, ref longer);

			void ShorterNet(ushort a, ushort b, out ushort shorter, out ushort longer)
			{
				bool a_is_shorter = _nets[b].CountUnits > _nets[a].CountUnits;
				shorter = a_is_shorter ? a : b;
				longer = a_is_shorter ? b : a;
			}
		}

		private void NetsIntegralityCheck()
		{
			foreach (var netId in _netsToIntegralityCheck)
			{
				//PeekLogger.LogName(netId);
				if (_nets.ContainsKey(netId))
					_nets[netId].IntegralityCheck();
			}
			_netsToIntegralityCheck.Clear();
		}

		private ushort NewNet(Net net = null)
		{
			ushort netId = GetNetId();
			Net newNet = net is null
				? new()
				: net;
			newNet.NetId = netId;

			_nets.Add(netId, newNet);
			newNet.AddNodeEvent += AddOrUpdateConnectedUnit;
			newNet.CreateNewNetsEvent += CreateNewNetsEvent;
			newNet.DestroyNet += DestroyNet;
			newNet.OnRemovingLink += RemoveLink;
			newNet.UnitsToReconnect += UnitsToReconnect;

			return netId;
		}

		private void ReconnectAll()
		{
			UnitId unitId;
			while (_unitsToReconnection.Count > 0)
			{
				unitId = _unitsToReconnection.Pop();
				PeekLogger.LogName(unitId);
				if (!unitId.IsPowerSource)
					SingleConnections(unitId);
			}
		}

		private void RemoveLink((UnitId, UnitId) ids)
		{
			//PeekLogger.LogWarning($"removing link in cdb {ids}");
			Link link = _links.Single(x => x.IsConnectionEqual(ids.Item1, ids.Item2));
			_ = _links.Remove(link);
		}

		private void SingleConnections(UnitId unitId)
		{
			if (ConnectionProcessor
				.GetSingleConnections(unitId, out GridPoint unitPosition, out var reconnections)
				)
				ConnectOneWithMany(unitId, unitPosition, ref reconnections);
		}

		private void SuperClusterConnections(UnitId unitId)
		{
			//levels 1-4
			IEnumerable<(UnitId, GridPoint, UnitId, GridPoint)> allConnections =
				ConnectionProcessor.GetAllSuperClusterConnections(unitId);
			foreach (var (idA, positionA, idB, positionB) in allConnections)
			{
				MakeConnection(idA, positionA, idB, positionB);
			}
		}

		private void ToReconnection(UnitId unitId)
			=> _unitsToReconnection.Push(unitId);

		private void UnitsToReconnect(IEnumerable<UnitId> units)
		{
			foreach (UnitId unit in units)
				ToReconnection(unit);
		}

		private void UpdateConnectedUnitsList(Net net)
		{
			ushort netId = net.NetId;
			foreach (UnitId id in net.UnitsIds)
				AddOrUpdateConnectedUnit(id, netId);
		}
	}
}