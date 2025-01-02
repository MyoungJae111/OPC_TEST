using Opc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_TEST.Model.Service
{
    public class ServerService
    {
        public void Connect()
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
    }
}
