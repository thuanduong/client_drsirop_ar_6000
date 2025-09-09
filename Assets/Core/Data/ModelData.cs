using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Core.Model
{
    public abstract class ModelData
    {
        protected Dictionary<string, object> _dictData = new Dictionary<string, object>();
        protected bool _inStreamingAssets = false;
        static int lastId = 0;

        public List<string> GetAllModelName()
        {
            List<string> list = new List<string>();
            foreach (var item in _dictData)
            {
                list.Add(item.Key);
            }
            return list;
        }

        public void Drop<T>(bool hasCreate = false)
        {
            var name = typeof(T).Name;
            object obj;
            if (_dictData.TryGetValue(name, out obj))
            {
                _dictData[name] = new List<T>();
            }
        }

        public void RegistModelData<T>() where T : BaseModel, new()
        {
            var name = typeof(T).Name;
            if (!_dictData.ContainsKey(name))
            {
                _dictData.Add(name, new List<T>());
            }
        }

        protected List<T> GetData<T>() where T : BaseModel
        {
            var name = typeof(T).Name;
            object obj;
            if (_dictData.TryGetValue(name, out obj))
            {
                var list = obj as List<T>;
                return list;
            }
            return new List<T>();
        }

        public List<T> GetAll<T>() where T : BaseModel, new()
        {
            var list = GetData<T>().ToList();
            return list;
        }

        public List<T> GetList<T>(Func<T, bool> func, bool isClone = false) where T : BaseModel, new()
        {
            var all = GetAll<T>();
            var list = all.Where(x => func(x)).ToList();
            if (isClone)
            {
                list = list.Clone();
            }
            return list;
        }

        public List<T> GetListByListId<T>(List<string> listId, bool isClone = false, bool ignoreNull = false) where T : BaseModel, new()
        {
            var all = GetAll<T>();
            List<T> list = new List<T>();
            if (listId == null)
                return list;
            for (int i = 0; i < listId.Count; i++)
            {
                var id = listId[i];
                if (string.IsNullOrEmpty(id))
                {
                    if (!ignoreNull)
                    {
                        list.Add(null);
                    }
                    continue;
                }
                var t = all.GetByHashCode(id.GetHashCode());
                if (t != null)
                {
                    if (isClone)
                    {
                        t = (T)t.Clone();
                    }
                    list.Add(t);
                }
                else if (!ignoreNull)
                {
                    list.Add(null);
                }
            }
            return list;
        }

        public T GetOne<T>() where T : BaseModel, new()
        {
            var all = GetAll<T>();

            if (all.Count > 0)
            {
                return all[0];
            }
            return null;
        }

        public T Get<T>(string id, bool isClone = false) where T : BaseModel, new()
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            var all = GetAll<T>();
            var t = all.GetByHashCode(id.GetHashCode());
            if (isClone)
            {
                t = (T)t.Clone();
            }
            return t;
        }

        public T Get<T>(Func<T, bool> func, bool isClone = false) where T : BaseModel, new()
        {
            var all = GetAll<T>();
            var t = all.FirstOrDefault(x => func(x));
            if (isClone)
            {
                t = (T)t.Clone();
            }
            return t;
        }

        public void Insert<T>(T t, Action<T> onComplete = null) where T : BaseModel, new()
        {
            var all = GetData<T>();
            if (string.IsNullOrEmpty(t.Id))
            {
                t.Id = GenerateID();
            }
            all.AddHasSort(t);
            if (onComplete != null)
            {
                onComplete(t);
            }
        }

        public void Insert<T>(List<T> list, Action<List<T>> onComplete = null) where T : BaseModel, new()
        {
            var all = GetData<T>();
            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                if (string.IsNullOrEmpty(t.Id))
                {
                    t.Id = GenerateID();
                }
            }
            all.AddRangeHasSort(list);
            if (onComplete != null)
            {
                onComplete(list);
            }
        }

        public void Update<T>(T t, Action<T> onComplete = null, bool hasClearCache = false) where T : BaseModel, new()
        {
            var all = GetData<T>();
            var data = all.GetByHashCode(t.GetHashCode());
            if (data != null)
            {
                if (data != t)
                {
                    data = JsonUtility.FromJson<T>(t.GetJson());
                }
                if (hasClearCache)
                {
                    data.ClearCache();
                }
                data.OnChangedCommand.Execute();
            }
            if (onComplete != null)
            {
                onComplete(t);
            }
        }

        public void Update<T>(List<T> list, Action<List<T>> onComplete = null, bool hasClearCache = false) where T : BaseModel, new()
        {
            var all = GetData<T>();
            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                var data = all.GetByHashCode(t.GetHashCode());
                if (data != null)
                {
                    if (data != t)
                    {
                        data = JsonUtility.FromJson<T>(t.GetJson());
                    }
                    if (hasClearCache)
                    {
                        data.ClearCache();
                    }
                    data.OnChangedCommand.Execute();
                }
            }
            if (onComplete != null)
            {
                onComplete(list);
            }
        }

        #region use for get table data
        public void InsertOrUpdate<T>(T t, Action onComplete = null, bool hasClearCache = false) where T : BaseModel, new()
        {
            var all = GetData<T>();
            T data = null;
            if (!string.IsNullOrEmpty(t.Id))
            {
                data = all.GetByHashCode(t.GetHashCode());
            }
            if (data != null)
            {
                if (data != t)
                {
                    data = JsonUtility.FromJson<T>(t.GetJson());
                }
                data.OnChangedCommand.Execute();
                Update(t, null, hasClearCache);
            }
            else
            {
                Insert(t, null);
            }
            if (onComplete != null)
            {
                onComplete();
            }
        }

        public void InsertOrUpdate<T>(List<T> list, Action onComplete = null, bool hasClearCache = false) where T : BaseModel, new()
        {
            var all = GetData<T>();
            var listInsert = new List<T>();
            var listUpdate = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                var t = list[i];
                T data = null;
                if (!string.IsNullOrEmpty(t.Id))
                {
                    data = all.GetByHashCode(t.GetHashCode());
                }
                if (data == null)
                {
                    listInsert.Add(t);
                }
                else
                {
                    listUpdate.Add(t);
                }
            }
            if (listInsert.Count > 0)
            {
                Insert(listInsert, null);
            }
            if (listUpdate.Count > 0)
            {
                Update(listUpdate, null, hasClearCache);
            }
            if (onComplete != null)
            {
                onComplete();
            }
        }
        #endregion

        public void Delete<T>(T t, Action onComplete = null) where T : BaseModel, new()
        {
            if (t == null)
            {
                if (onComplete != null)
                {
                    onComplete();
                }
                return;
            }
            var all = GetData<T>();
            var data = all.GetByHashCode(t.GetHashCode());
            if (data != null)
            {
                all.Remove(data);
            }
            if (onComplete != null)
            {
                onComplete();
            }
        }

        public void Delete<T>(List<T> list, Action onComplete = null) where T : BaseModel, new()
        {
            var all = GetData<T>();
            var data = all.Where(x =>
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var record = list[i];
                    if (record != null && record.Id == x.Id)
                    {
                        list.Remove(record);
                        return true;
                    }
                }
                return false;
            }).ToList();
            for (int i = 0; i < data.Count; i++)
            {
                var record = data[i];
                all.Remove(record);
            }
            if (onComplete != null)
            {
                onComplete();
            }
        }

        public static string GenerateID()
        {
            var id = Interlocked.Increment(ref lastId);
            return id.ToString();
        }

        public virtual void ClearData()
        {
            _dictData = new Dictionary<string, object>();
        }

        public void ClearCache<T>() where T : BaseModel, new()
        {
            var all = GetData<T>();
            all.Clear();
        }
    }
}