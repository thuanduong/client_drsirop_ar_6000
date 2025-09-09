using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Core.Model
{
    public class BaseModel : OriginalModel
    {
        public string Id { get; set; }

        protected int _hashCode = -1;
        public override int GetHashCode()
        {
            if (_hashCode == -1)
            {
                _hashCode = Id.GetHashCode();
                if (_hashCode == -1)
                {
                    _hashCode = Guid.NewGuid().GetHashCode();
                }
            }
            return _hashCode;
        }

        #region binding
        public ReactiveCommand OnChangedCommand = new ReactiveCommand();

        public void UnbindOnChanged()
        {
            OnChangedCommand.Dispose();
            OnChangedCommand = new ReactiveCommand();
        }
        #endregion
    }
}