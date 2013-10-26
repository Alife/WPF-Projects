using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.ComponentModel;

namespace ID3.ID3v2Frames
{
    public class FilterCollection
    {
        private ArrayList _Frames;

        internal FilterCollection()
        {
            _Frames = new ArrayList();
        }

        /// <summary>
        /// ���FrameID��FrameList
        /// </summary>
        /// <param name="FrameID">Ҫ��ӵ�FrameID</param>
        public void Add(string FrameID)
        {
            if (!_Frames.Contains(FrameID))
                _Frames.Add(FrameID);
        }

        /// <summary>
        /// ɾ��ȷ����FrameID
        /// </summary>
        /// <param name="FrameID">Ҫɾ����FrameID</param>
        public void Remove(string FrameID)
        {
            _Frames.Remove(FrameID);
        }

        /// <summary>
        /// ��ȡFrames
        /// </summary>
        public string[] Frames
        {
            get
            { return (string[])_Frames.ToArray(typeof(string)); }
        }

        /// <summary>
        /// ɾ��ȫ����Frame
        /// </summary>
        public void Clear()
        {
            _Frames.Clear();
        }

        /// <summary>
        /// �ж�Frame�Ƿ����
        /// </summary>
        /// <param name="FrameID">FrameID to search</param>
        /// <returns>�Ƿ����</returns>
        public bool IsExists(string FrameID)
        {
            if (_Frames.Contains(FrameID))
                return true;
            else
                return false;
        }
    }

    public class FramesCollection<T>
    {
        private ArrayList _Items; // �洢���е�����- -

        /// <summary>
        /// Frames Collection
        /// </summary>
        internal FramesCollection()
        {
            _Items = new ArrayList();
        }

        /// <summary>
        /// ���
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            // ������ڣ�����ɾ��
            _Items.Remove(item);

            _Items.Add(item);
        }

        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="item">Item to remove</param>
        public void Remove(T item)
        {
            _Items.Remove(item);
        }

        /// <summary>
        /// ����ȷ����λ��ɾ��
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            _Items.RemoveAt(index);
        }

        /// <summary>
        /// ���
        /// </summary>
        public void Clear()
        {
            _Items.Clear();
        }

        /// <summary>
        /// Array of items
        /// </summary>
        public T[] Items
        {
            get
            { return (T[])_Items.ToArray(typeof(T)); }
        }

        /// <summary>
        /// ��ȡ�ܳ���
        /// </summary>
        public int Length
        {
            get
            {
                int Len = 0;
                foreach (ILengthable IL in _Items)
                    Len += IL.Length;
                return Len;
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        public void Sort()
        {
            _Items.Sort();
        }

        /// <summary>
        /// ��ǰFramesCollection����Ŀ
        /// </summary>
        public int Count
        {
            get
            { return _Items.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return _Items.GetEnumerator();
        }
    }
}
