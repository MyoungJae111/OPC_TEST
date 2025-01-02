using Opc.Da;
using Opc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_TEST
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IDiscovery discovery = new OpcCom.ServerEnumerator();
            Opc.Server[] servers = discovery.GetAvailableServers(Specification.COM_DA_20);

            if (servers.Length == 0)
            {
                Console.WriteLine("사용 가능한 OPC 서버가 없습니다.");
                Console.ReadKey();
                return;
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

                // 브라우징 및 데이터 읽기 수행
                BrowseItems(server);
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

        static void BrowseItems(Opc.Da.Server server)
        {
            try
            {
                // 브라우징 필터 설정
                BrowseFilters filters = new BrowseFilters();
                filters.BrowseFilter = browseFilter.all;

                // 루트 노드에서 브라우징 시작
                BrowsePosition position;
                BrowseElement[] elements = server.Browse(null, filters, out position);

                Console.WriteLine("아이템 리스트:");
                foreach (var element in elements)
                {
                    if (element.IsItem)
                    {
                        Console.WriteLine($"아이템 이름: {element.ItemName}, 설명: {element.ItemPath}");
                        ReadItem(server, element.ItemName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"브라우징 중 오류 발생: {ex.Message}");
            }
        }

        static void ReadItem(Opc.Da.Server server, string itemName)
        {
            try
            {
                // 읽을 아이템 설정
                Item item = new Item { ItemName = itemName };

                // 데이터 읽기
                ItemValueResult result = server.Read(new Item[] { item })[0];

                if (result.ResultID.Succeeded())
                {
                    Console.WriteLine($"아이템 값: {result.Value}, 타임스탬프: {result.Timestamp}, 품질: {result.Quality}");
                }
                else
                {
                    Console.WriteLine("아이템 값을 읽는 데 실패했습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"데이터 읽기 중 오류 발생: {ex.Message}");
            }
        }
    }
}
