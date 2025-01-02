using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPC_TEST.Model
{
    public class OpcNode
    {
        // 노드 이름 (예: 태그 이름)
        public string Name { get; set; }

        // 노드의 고유 식별자 (OPC Item ID)
        public string ItemId { get; set; }

        // 하위 노드(자식들)
        public List<OpcNode> Children { get; set; } = new List<OpcNode>();

        // 생성자
        public OpcNode(string name, string itemId)
        {
            Name = name;
            ItemId = itemId;
        }

        // 자식 노드 추가
        public void AddChild(OpcNode child)
        {
            Children.Add(child);
        }
    }
}
