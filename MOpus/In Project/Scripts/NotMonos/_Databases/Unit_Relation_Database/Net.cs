using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace NotMonos.Databases
{
	internal sealed partial class ConnectionsDB
	{
		private sealed class Net
		{
			private readonly SortedDictionary<ushort, Link> _links = new();
			private readonly Dictionary<UnitId, NetNode> _nodes = new(2);
			private readonly Stack<ushort> _removedLinkIds = new();

			internal Net()
			{ }

			private Net(Dictionary<UnitId, NetNode> nodes, SortedDictionary<ushort, Link> links)
				=> (_nodes, _links) = (nodes, links);

			internal event Action<UnitId, ushort> AddNodeEvent;

			internal event Action<IEnumerable<Net>> CreateNewNetsEvent;

			internal event Action<ushort> DestroyNet;

			internal event Action<(UnitId, UnitId)> OnRemovingLink;

			internal event Action<IEnumerable<UnitId>> UnitsToReconnect;

			internal int CountUnits
				=> _nodes.Count;

			internal IEnumerable<(GridPoint, GridPoint)> LinksPoints
				=> from l in _links.Values
				   select l.GetPoints();

			internal ushort NetId { get; set; }

			internal IEnumerable<UnitId> UnitsIds
				=> _nodes.Keys;

			internal void AddLink(Link link)
			{
				ushort linkId = GetLinkId();
				//PeekLogger.Log($"net {NetId} add link {linkId} {link.GetUnits()}");
				_links.Add(linkId, link);
				var (a, b) = link.GetUnits();
				_nodes[a].Add(linkId);
				_nodes[b].Add(linkId);
			}

			internal void AddUnits(params UnitId[] unitIds)
			{
				foreach (var item in unitIds)
				{
					if (_nodes.ContainsKey(item))
						continue;
					_nodes.Add(item, new());
					AddNodeEvent.SafeInvoke(item, NetId);
				}
			}

			internal void GetNeighborsTo(UnitId unitId, out UnitId[] neighbors)
				=> neighbors = (from linkId in _nodes[unitId].Links
								select _links[linkId].GetNeighborOf(unitId))
								.ToArray();

			internal bool HaveUnit(UnitId id)
				=> _nodes.ContainsKey(id);

			internal void IntegralityCheck()
			{
				int nodesCount = _nodes.Count,
					linksCount;// = _links.Count;
				if (nodesCount <= 1)// || linksCount == 0) //todo need to reconsider
				{
					PeekLogger.LogTab($"destroy event on net {NetId}");
					DestroyNet.SafeInvoke(NetId);
					return; //this net is not exist
				}

				IEnumerable<UnitId> unitsIds;
				IEnumerable<ushort> linksIds;
				List<Net> nets = new();
				bool isSameNodesCount, isSameLinksCount;

				do
				{
					BFS(_nodes.Keys.First(), out unitsIds, out linksIds);
					nodesCount = unitsIds.Count();
					linksCount = linksIds.Count();

					bool isHaveNewNets = TryCreateNet(nodesCount, linksCount, unitsIds, linksIds, out Net net);
					if (isHaveNewNets)
					{
						nets.Add(net);
					}

					if (_nodes.Count <= 1 || (!isHaveNewNets && _nodes.Count == 2))// && _links.Count == 0)
					{
						DestroyNet.SafeInvoke(NetId);
						break;
					}

					isSameNodesCount = nodesCount != _nodes.Count;
					isSameLinksCount = _links.Count != linksCount;
				} while (isSameNodesCount && isSameLinksCount);

				if (nets.Count > 0)
				{
					CreateNewNetsEvent.SafeInvoke(nets);
				}
			}

			internal void MergeWith(Net net)
			{
				PeekLogger.LogName(net.NetId);
				UnitId[] unitsInOtherNet = net._nodes.Keys.ToArray();
				AddUnits(unitsInOtherNet);
				PeekLogger.LogItems(unitsInOtherNet);

				foreach (var link in net._links.Values)
					AddLink(link);
			}

			internal void RemoveUnit(UnitId unitId)
			{
				if (!_nodes.ContainsKey(unitId))
					return;

				var unitLinks = _nodes[unitId].Links;
				var nodesForRemoveLinks =
					from linkId in unitLinks
					select (_links[linkId].GetNeighborOf(unitId), linkId);
				//PeekLogger.LogItems(nodesForRemoveLinks);

				foreach (var (node, link) in nodesForRemoveLinks)
				{
					_nodes[node].RemoveLink(link);
					RemoveLink(link);
				}
				RemoveNode(unitId);
			}

			/*internal void RemoveUnits(UnitId[] disconnected)
			{
				PeekLogger.LogMessageTabTab("removing units in "+NetId);
				var disconnectedLinks =
					(from uid in disconnected
					 from lid in _nodes[uid].Links
					 select lid).Distinct();
				PeekLogger.LogItems(disconnected);
				PeekLogger.LogItems(disconnectedLinks);

				var neighborsOfDisconnected =
					(from linkId in disconnectedLinks
					from uid in disconnected
					let check = new
					{
						HaveNeighbor = _links[linkId].TryGetNeighborOf(uid, out var neighborId),
						NeighborId = neighborId
					}
					where check.HaveNeighbor && !disconnected.Contains(check.NeighborId)
					select (check.NeighborId, linkId)).ToArray();

				PeekLogger.LogItems(neighborsOfDisconnected);
				HashSet<UnitId> onReconnection = new();
				foreach (var (node, link) in neighborsOfDisconnected)
				{
					_nodes[node].RemoveLink(link);
					RemoveLink(link);
					onReconnection.Add(node);
				}
				foreach (var item in disconnected)
				{
					RemoveNode(item);
				}
				UnitsToReconnect.SafeInvoke(onReconnection);
			}*/

			internal void RemoveUnits(UnitId[] disconnected)
			{
				var disconnectedLinks =
					(from uid in disconnected
					 from lid in _nodes[uid].Links
					 select lid).Distinct();

				PeekLogger.LogItems(disconnected);
				PeekLogger.LogItems(disconnectedLinks);

				var neighborsOfDisconnected =
					(from linkId in disconnectedLinks
					 from uid in disconnected
					 let check = new
					 {
						 HaveNeighbor = _links[linkId].TryGetNeighborOf(uid, out var neighborId),
						 NeighborId = neighborId
					 }
					 where check.HaveNeighbor && !disconnected.Contains(check.NeighborId)
					 select (check.NeighborId, linkId)).ToArray();//.Except(UnitId.PowerSource). ?
																  //Convert to array is important!
																  //IEnumerable has Deferred execution, it leads to bug,
																  //because next foreach removing link
				PeekLogger.LogItems(neighborsOfDisconnected);
				HashSet<UnitId> onReconnection = new();

				foreach (var (id, link) in neighborsOfDisconnected)
				{
					_ = onReconnection.Add(id);
					_nodes[id].RemoveLink(link); //preventing wild Ids in this Node (link is deleted, but Node still have it)
				}

				foreach (var item in disconnectedLinks)
					RemoveLink(item);

				foreach (var item in disconnected)
					RemoveNode(item);

				UnitsToReconnect.SafeInvoke(onReconnection);
			}

			private void BFS(UnitId start, out IEnumerable<UnitId> unitsIds, out IEnumerable<ushort> linksIds)
			{
				HashSet<UnitId> usedNodes = new(); // graph
				Stack<ushort> links = new();
				Queue<UnitId> nodesOrder = new();
				nodesOrder.Enqueue(start);

				PeekLogger.LogName(start);

				while (nodesOrder.Count > 0)
				{
					var id = nodesOrder.Dequeue();
					//PeekLogger.Log(id);
					SearchNeighbors(id);
				}
				unitsIds = usedNodes;
				linksIds = links;

				void SearchNeighbors(UnitId unitId)
				{
					UnitId neighbor;
					//PeekLogger.LogItems(_nodes[unitId].Links);
					foreach (ushort linkId in _nodes[unitId].Links) //.ToArray())
					{
						/*if (!_links.ContainsKey(linkId)) //this is defence against wild linkIds in Nodes
						{
							_nodes[unitId].RemoveLink(linkId);
							continue;
						}*/
						if (!links.Contains(linkId))
						{
							//PeekLogger.Log($"push link {linkId}");
							links.Push(linkId);
							neighbor = _links[linkId].GetNeighborOf(unitId);
							//PeekLogger.Log("order + "+neighbor);
							nodesOrder.Enqueue(neighbor);
						}
					}
					usedNodes.Add(unitId);
					//PeekLogger.Log("used push"+unitId);
				}
			}

			private ushort GetLinkId()
			{
				ushort value;
				if (_removedLinkIds.Count == 0)
				{
					value = _links.LastOrDefault().Key;
					value++;
				}
				else
					value = _removedLinkIds.Pop();

				return value;
			}

			private void RemoveLink(ushort linkId)
			{
				//PeekLogger.LogWarning($"removing link {linkId} in {NetId}");
				OnRemovingLink.SafeInvoke(_links[linkId].GetUnits());
				_links.Remove(linkId);
				_removedLinkIds.Push(linkId);
			}

			private void RemoveNode(UnitId unitId)
				=> _nodes.Remove(unitId);

			private bool TryCreateNet(int nodesCount,
							 int linksCount,
							 IEnumerable<UnitId> unitsIds,
							 IEnumerable<ushort> linksIds,
							 out Net net)
			{
				net = null;
				if (linksCount == 0)
					return false;

				Dictionary<UnitId, NetNode> nodes = new(nodesCount);
				SortedDictionary<ushort, Link> links = new();

				//PeekLogger.LogItems(unitsIds);
				//PeekLogger.LogItems(linksIds);

				foreach (var item in unitsIds)
				{
					nodes.Add(item, _nodes[item]);
					_ = _nodes.Remove(item);
				}
				foreach (var item in linksIds)
				{
					links.Add(item, _links[item]);
					_ = _links.Remove(item);
				}

				net = new(nodes, links);
				return true;
			}
		}
	}
}