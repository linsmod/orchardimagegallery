using System.Linq;
using System.Web;
using Autofac;
using Mello.ImageGallery.Controllers;
using Mello.ImageGallery.Models;
using Mello.ImageGallery.Services;
using Mello.ImageGallery.ViewModels;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Moq;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace Mello.ImageGallery.Tests {
    [TestFixture]
    public class AdminControllerTests {
        private IContainer _container;
        private AdminController _adminController;
        private Mock<IImageGalleryService> _imageGalleryServiceMock;

        [SetUp]
        public void Init() {
            _imageGalleryServiceMock = new Mock<IImageGalleryService>();
            var orchardServicesMock = new Mock<IOrchardServices>();
            orchardServicesMock.Setup(
                o => o.Authorizer.Authorize(Permissions.ManageImageGallery, It.IsAny<LocalizedString>())).Returns(true);
            orchardServicesMock.Setup(o => o.Notifier.Add(It.IsAny<NotifyType>(), It.IsAny<LocalizedString>()));

            var builder = new ContainerBuilder();
            builder.RegisterInstance(_imageGalleryServiceMock.Object).As<IImageGalleryService>();
            builder.RegisterType<AdminController>().As<AdminController>();
            builder.RegisterInstance(orchardServicesMock.Object).As<IOrchardServices>();


            _container = builder.Build();

            _adminController = _container.Resolve<AdminController>();
        }

        [Test]
        public void Index_Should_Return_ImageGalleryIndexViewModel() {
            // Arrange
            _imageGalleryServiceMock.Setup(galleryService => galleryService.GetImageGalleries()).Returns(
                TestUtils.GetGalleries(1)).Verifiable();

            // Act
            var result = _adminController.Index();

            // Assert
            result.AssertViewRendered().ForView("").WithViewData<ImageGalleryIndexViewModel>();
            Assert.That((result.Model as ImageGalleryIndexViewModel).ImageGalleries.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Create_Should_Return_CreateGalleryViewModel() {
            // Act
            var result = _adminController.Create();

            // Assert
            result.AssertViewRendered().ForView("").WithViewData<CreateGalleryViewModel>();
        }

        [Test]
        public void Create_Post_Should_Add_Gallery_And_Redirect_To_Index() {
            // Arrange
            var viewModel = new CreateGalleryViewModel() {GalleryName = "NewGallery"};
            _imageGalleryServiceMock.Setup(o => o.CreateImageGallery("NewGallery")).Verifiable();

            // Act
            var result = _adminController.Create(viewModel);

            // Assert
            _imageGalleryServiceMock.Verify();
            result.AssertActionRedirect().ToAction("Index");
        }

        [Test]
        public void Images_Should_Return_ImageGalleryEditImagesViewModel() {
            // Arrange
            _imageGalleryServiceMock.Setup(galleryService => galleryService.GetImageGallery("gallery")).Returns(
                new Mello.ImageGallery.Models.ImageGallery {Name = "gallery"});

            // Act
            var result = _adminController.Images("gallery");

            // Assert
            result.AssertViewRendered().ForView("").WithViewData<ImageGalleryImagesViewModel>();
            ImageGalleryImagesViewModel model = ((ViewResult) result).Model as ImageGalleryImagesViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("gallery", model.ImageGalleryName);
        }

        [Test]
        public void EditProperties_Should_Return_ImageGalleryEditPropertiesViewModel() {
            // Arrange
            _imageGalleryServiceMock.Setup(galleryService => galleryService.GetImageGallery("gallery")).Returns(
                new Models.ImageGallery {Name = "gallery"});

            // Act
            var result = _adminController.EditProperties("gallery");

            // Assert
            result.AssertViewRendered().ForView("").WithViewData<ImageGalleryEditPropertiesViewModel>();
            ImageGalleryEditPropertiesViewModel model =
                ((ViewResult) result).Model as ImageGalleryEditPropertiesViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("gallery", model.ImageGallery.Name);
        }

        [Test]
        public void EditProperties_Post_Should_Save_And_Return_To_Images() {
            // Arrange
            _imageGalleryServiceMock.Setup(o => o.UpdateImageGalleryProperties("gallery", 80, 100, false, false)).Verifiable();
            var imageGallery = new Models.ImageGallery
                               {Name = "gallery", ThumbnailHeight = 80, ThumbnailWidth = 100};
            ImageGalleryEditPropertiesViewModel viewModel = new ImageGalleryEditPropertiesViewModel
                                                            {ImageGallery = imageGallery};

            // Act        
            var result = _adminController.EditProperties(viewModel, "gallery");

            // Assert
            _imageGalleryServiceMock.Verify();
            result.AssertActionRedirect().ToAction("Images");
        }

        [Test]
        public void AddImages_Should_Return_ImageAddViewModel() {
            // Arrange

            // Act
            var result = _adminController.AddImages("gallery");

            // Assert
            result.AssertViewRendered().ForView("").WithViewData<ImageAddViewModel>();
            ImageAddViewModel model = ((ViewResult) result).Model as ImageAddViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("gallery", model.ImageGalleryName);
        }

        [Test]
        public void AddImages_Post_Should_Add_Image_To_Gallery_And_Return_To_Gallery_Images() {
            // Arrange
            var fileMock = TestUtils.GetMockImagePostedFile();
            _imageGalleryServiceMock.Setup(o => o.AddImage("gallery", fileMock)).Verifiable();
            _imageGalleryServiceMock.Setup(o => o.IsFileAllowed(It.Is<HttpPostedFileBase>(file => file == fileMock))).
                Returns(true).Verifiable();

            // Act
            var result =
                _adminController.AddImages(new ImageAddViewModel
                                           {ImageGalleryName = "gallery", ImageFiles = new[] {fileMock}});

            // Assert
            result.AssertActionRedirect().ToAction("Images");
            _imageGalleryServiceMock.Verify();
        }

        [Test]
        public void EditImage_Should_Return_ImageEditViewModel() {
            // Arrange
            ImageGalleryImage image = new ImageGalleryImage {Name = "image1"};
            _imageGalleryServiceMock.Setup(imageGalleryService => imageGalleryService.GetImage("gallery", "image1")).
                Returns(image).Verifiable();

            // Act
            var result = _adminController.EditImage("gallery", "image1");

            // Assert
            _imageGalleryServiceMock.Verify();
            result.AssertViewRendered().ForView("").WithViewData<ImageEditViewModel>();
            ImageEditViewModel model = ((ViewResult) result).Model as ImageEditViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual("gallery", model.ImageGalleryName);
            Assert.AreEqual("image1", model.Image.Name);
        }

        [Test]
        public void DeleteImage_Should_Delete_And_Return_To_Gallery_Images() {
            // Arrange
            _imageGalleryServiceMock.Setup(imageGalleryService => imageGalleryService.DeleteImage("gallery", "image")).
                Verifiable();

            // Act
            var result = _adminController.DeleteImage("gallery", "image");

            // Assert
            _imageGalleryServiceMock.Verify();
            result.AssertActionRedirect().ToAction("Images");
        }

        [Test]
        public void Delete_Should_Delete_Gallery_And_Redirect_To_Index() {
            // Arrange
            _imageGalleryServiceMock.Setup(imageGalleryService => imageGalleryService.DeleteImageGallery("gallery")).
                Verifiable();

            // Act
            var result = _adminController.Delete("gallery");

            // Assert
            _imageGalleryServiceMock.Verify();
            result.AssertActionRedirect().ToAction("Index");
        }

        [Test]
        public void CanReorderImages() {
            var images = new string[] {"image"};

            // Arrange
            _imageGalleryServiceMock.Setup(o => o.ReorderImages("gallery", images)).Verifiable();

            // Act
            var result = _adminController.Reorder("gallery", images);

            // Assert
            _imageGalleryServiceMock.Verify();
        }
    }
}