using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MusicSorter
{
    /// <summary>
    /// 获取MP3文件的ID3 V1版本的TAG信息的类
    /// </summary>
    internal class TagLibHelper
    {
        private string FileName;

        public TagLib.File file;

        public TagLibHelper(string FileName)
        { 
            this.FileName = FileName;
            file = TagLib.File.Create(FileName);
        }

        public string Lyrics { get; set; }

        public TagLib.IPicture[] Pictures { get; set; }

        public void SaveLyrics(string Lyrics)
        {
            if (file == null) return;
            file.Tag.Lyrics = Lyrics;
            Save();
        }
        public void SavePictures(TagLib.IPicture[] Pictures)
        {
            if (file == null) return;
            file.Tag.Pictures = Pictures;
            Save();
        }
        public void Save()
        {
            if (file == null) return;
            file.Save();
        }
    }
}
