using System.Collections.Generic;
using System.Web;

using Mello.ImageGallery.Models;

using Orchard.MediaLibrary.Models;


namespace Mello.ImageGallery.Tests {
    public static class TestUtils {
        public static List<Models.ImageGallery> GetGalleries(int count) {
            var galleries = new List<Models.ImageGallery>();
            for (int i = 0; i < count; i++) {
                galleries.Add(new Models.ImageGallery {Name = i.ToString()});
            }

            return galleries;
        }

        public static List<MediaFolder> GetMediaFolders(int count) {
            var folders = new List<MediaFolder>();
            folders.Add(new MediaFolder {Name = "gallery", MediaPath = "ImageGalleries"});
            for (int i = 1; i < count; i++) {
                folders.Add(new MediaFolder {Name = i.ToString(), MediaPath = "ImageGalleries" + i});
            }

            return folders;
        }

        public static List<MediaFile> GetMediaFiles(int count) {
            var folders = new List<MediaFile>();
            for (int i = 0; i < count; i++) {
                folders.Add(new MediaFile {Name = "image" + i, FolderName = "gallery"});
            }

            return folders;
        }

        public static ImageGallerySettingsRecord GetImageGallerySettingsRecord() {
            List<ImageGalleryImageSettingsRecord> images = new List<ImageGalleryImageSettingsRecord>();
            images.Add(new ImageGalleryImageSettingsRecord {Caption = "caption", Name = "image1"});
            return new ImageGallerySettingsRecord {ImageSettings = images};
        }

        public static HttpPostedFileBase GetMockImagePostedFile() {
            return new MockImagePostedFile();
        }

        private class MockImagePostedFile : HttpPostedFileBase {
            public override string ContentType {
                get { return "image/PNG"; }
            }

            public override string FileName {
                get { return "image"; }
            }
        }
    }
}