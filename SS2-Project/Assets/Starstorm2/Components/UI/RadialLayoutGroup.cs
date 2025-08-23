using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
namespace SS2.UI
{
    public class RadialLayoutGroup : MonoBehaviour, ILayoutElement, ILayoutGroup // LOL NEVERMIND FUCK THIS
    {
        public float startAngle;
        public float maxAngle = 360f;
        public float distance;
        public void CalculateLayoutInputHorizontal()
        {
            throw new NotImplementedException();
        }

        public void CalculateLayoutInputVertical()
        {
            throw new NotImplementedException();
        }

        public void SetLayoutHorizontal()
        {
            throw new NotImplementedException();
        }

        public void SetLayoutVertical()
        {
            throw new NotImplementedException();
        }

        public float minWidth => minWidth;
        private float _minWidth;
        public float preferredWidth => preferredWidth;
        private float _preferredWidth;
        public float flexibleWidth => flexibleWidth;
        private float _flexibleWidth;
        public float minHeight => minHeight;
        private float _minHeight;
        public float preferredHeight => preferredHeight;
        private float _preferredHeight;
        public float flexibleHeight => _flexibleHeight;
        private float _flexibleHeight;
        public int layoutPriority => _layoutPriority;
        private int _layoutPriority;

    }
}
