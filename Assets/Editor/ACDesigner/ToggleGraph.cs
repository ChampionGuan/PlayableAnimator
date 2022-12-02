using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace ACDesigner
{
    public interface IToggle
    {
        bool IsShow { get; set; }
        void Show();
        void Hide();
    }

    public class ToggleGraph
    {
        private List<IToggle> m_graphs = new List<IToggle>();

        public void Add(IToggle graph)
        {
            if (null == graph)
            {
                return;
            }

            if (m_graphs.Contains(graph))
            {
                return;
            }

            m_graphs.Add(graph);
        }

        public void Remove(IToggle graph)
        {
            if (null == graph)
            {
                return;
            }

            if (m_graphs.Contains(graph))
            {
                m_graphs.Remove(graph);
            }
        }

        public void Clear()
        {
            m_graphs.Clear();
        }

        public void Toggle(IToggle graph)
        {
            foreach (var child in m_graphs)
            {
                if (child != graph && child.IsShow)
                {
                    child.Hide();
                }
            }

            if (null != graph && !graph.IsShow)
            {
                graph.Show();
            }
        }
    }
}