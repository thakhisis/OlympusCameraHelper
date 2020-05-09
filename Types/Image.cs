
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OlympusCameraHelper.Types
{
    public enum NavigationEntityType {
        File, Directory, Image
    }

    public abstract class NavigationEntity {
        public NavigationEntity(string directoryName, string name, int num1, int num2, int num3, int num4) {
            this.DirectoryName = directoryName;
            this.Name = name;
            this.num1 = num1;
            this.num2 = num2;
            this.num3 = num3;
            this.num4 = num4;
        }

        public string DirectoryName {get;set;}
        public string Name {get;set;}
        public abstract NavigationEntityType Type { get; }
        public int num1;
        public int num2; 
        public int num3;
        public int num4;
    }

    public class Directory : NavigationEntity
    {
        public Directory(string directoryName, string name, int num1, int num2, int num3, int num4) : base (directoryName, name, num1, num2, num3, num4) {

        }

        public override NavigationEntityType Type => NavigationEntityType.Directory;
    }

    public class File : NavigationEntity
    {
        public File(string directoryName, string name, int num1, int num2, int num3, int num4) : base (directoryName, name, num1, num2, num3, num4) {

        }

        public override NavigationEntityType Type => NavigationEntityType.File;
    }

    public class Image : NavigationEntity {
        public Image(string directoryName, string name, int num1, int num2, int num3, int num4) : base (directoryName, name, num1, num2, num3, num4) {

        }

        public string Thumbnail { get;set;}
        public long Size { get;set;}

        public string FormattedSize => 
            Size > 1 * 1024 * 1024 * 1024 ? Size / (1024 * 1024 * 1024) + "GB" :
            Size > 1024 * 1024 ? Size / (1024 * 1024) + "MB" :
            Size > 1024 ? Size / (1024) + "KB" : Size + "B";
        public double? DownloadProgress {get;set;}
        public long DownloadCurrent {get;set;}
        public DownloadState State {get;set;}
        public string Error {get;set;}
        public override NavigationEntityType Type => NavigationEntityType.Image;

        public enum DownloadState {
            None, Enqueued, Downloading, Completed, Failed
        }
    }
}