using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Match3.Core
{
    public abstract class GameObject : AppObject
    {
        private bool isEnabled = true;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;

                if (isEnabled)
                    OnEnabled();
                else
                    OnDisabled();
            }
        }

        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
    }
}
