
using Opc.Da;
using Opc;

using System;
using System.Collections.Generic;

namespace OPC_TEST
{
    internal class Program
    {
        // 트리 구조를 저장할 클래스
        public class OpcNode
        {
            public string Name { get; set; } // 노드 이름
            public string ItemId { get; set; } // 노드 식별자
            public List<OpcNode> Children { get; set; } = new List<OpcNode>(); // 자식 노드 리스트

            public OpcNode(string name, string itemId)
            {
                Name = name;
                ItemId = itemId;
            }

            public void AddChild(OpcNode child)
            {
                Children.Add(child);
            }
        }

        static void Main(string[] args)
        {
            IDiscovery discovery = new OpcCom.ServerEnumerator();
            Opc.Server[] servers = discovery.GetAvailableServers(Specification.COM_DA_20, "localhost", null);
            Opc.Server[] hdaServers = discovery.GetAvailableServers(Specification.COM_HDA_10, "localhost", null);

            if (servers.Length == 0)
            {
                Console.WriteLine("사용 가능한 OPC 서버가 없습니다.");
                Console.ReadKey();
                return;
            }

            foreach (var s in hdaServers)
            {
                Console.WriteLine($"서버 이름: {s.Name}");
                Console.WriteLine($"서버 URL: {s.Url}");
                Console.WriteLine();
            }

            // 첫 번째 서버 선택 (필요에 따라 사용자 입력으로 변경 가능)
            Opc.Server selectedServer = servers[0];
            Console.WriteLine($"선택된 서버: {selectedServer.Name}");

            OpcCom.Factory factory = new OpcCom.Factory();
            Opc.Da.Server server = new Opc.Da.Server(factory, null);

            try
            {
                server.Connect(selectedServer.Url, new ConnectData(new System.Net.NetworkCredential()));
                Console.WriteLine("OPC 서버에 성공적으로 연결되었습니다.");

                // 트리 구조 생성 및 출력
                OpcNode rootTree = BuildTree(server);
                PrintTree(rootTree, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"서버 연결 실패: {ex.Message}");
            }
            finally
            {
                if (server.IsConnected)
                {
                    server.Disconnect();
                    Console.WriteLine("서버 연결이 해제되었습니다.");
                }
            }

            Console.ReadKey();
        }

        // 트리 구조 생성 함수
        static OpcNode BuildTree(Opc.Da.Server server)
        {
            try
            {
                Opc.Da.Server daServer = server;

                // 루트 노드 생성
                OpcNode rootNode = new OpcNode("Root", null);

                // 브라우징 필터 설정
                BrowseFilters filters = new BrowseFilters();
                filters.BrowseFilter = browseFilter.all;

                // 루트 노드에서 브라우징 시작
                BrowsePosition position;
                BrowseElement[] elements = daServer.Browse(null, filters, out position);

                foreach (var element in elements)
                {
                    // 현재 노드의 절대 경로 계산
                    string itemPath = element.ItemPath ?? element.Name;

                    // 현재 노드 생성
                    OpcNode childNode = new OpcNode(element.Name, itemPath);

                    // 하위 노드가 있으면 재귀적으로 탐색
                    if (element.HasChildren)
                    {
                        childNode.Children.AddRange(BuildSubTree(daServer, itemPath));
                    }

                    rootNode.AddChild(childNode);
                }

                return rootNode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"트리 생성 중 오류 발생: {ex.Message}");
                return null;
            }
        }

        // 하위 트리를 재귀적으로 생성하는 함수
        static List<OpcNode> BuildSubTree(Opc.Da.Server daServer, string parentPath)
        {
            List<OpcNode> nodes = new List<OpcNode>();

            try
            {
                BrowseFilters filters = new BrowseFilters();
                filters.BrowseFilter = browseFilter.all;

                BrowsePosition position;
                BrowseElement[] elements = daServer.Browse(new ItemIdentifier(parentPath), filters, out position);

                if(elements == null) return nodes; // 하위 노드가 없는 경우 (종료 조건

                foreach (var element in elements)
                {
                    // 현재 노드의 절대 경로 계산
                    string itemPath = element.ItemPath ?? $"{parentPath}.{element.Name}";

                    OpcNode node = new OpcNode(element.Name, itemPath);

                    if (element.HasChildren)
                    {
                        node.Children.AddRange(BuildSubTree(daServer, itemPath));
                    }

                    nodes.Add(node);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"하위 트리 생성 중 오류 발생: {ex.Message}");
            }

            return nodes;
        }

        // 트리 구조 출력 함수
        static void PrintTree(OpcNode node, int level)
        {
            if (node == null) return;

            Console.WriteLine($"{new string(' ', level * 2)}- {node.Name} (ID: {node.ItemId})");

            foreach (var child in node.Children)
            {
                PrintTree(child, level + 1);
            }
        }

    }
}