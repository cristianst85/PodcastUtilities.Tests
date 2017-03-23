using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PodcastUtilities.PortableDevices;

namespace PodcastUtilities.Tests {

    [TestFixture]
    public class PortableDeviceTests {

        private readonly string deviceName = "HTC One"; // Android 5.0.2
        private readonly string filePath = @"Internal storage\PodcastUtilities.Tests\test.bin";
        private readonly long fileSize = 10 * 1024 * 1024;

        [TearDown]
        public void TearDown() {
            Console.WriteLine(TestContext.CurrentContext.Test.Name);
            DeviceManager deviceManager = new DeviceManager();
            var devices = deviceManager.GetAllDevices();
            var myDevice = devices.Where(x => x.Name.Equals(deviceName)).First();
            Assert.DoesNotThrow(() => { myDevice.Delete(filePath); }, "Remove test file.");
            Assert.DoesNotThrow(() => { myDevice.Delete(filePath.Substring(0, filePath.LastIndexOf('\\'))); }, "Remove test directory.");
            var deviceObject = myDevice.GetObjectFromPath(filePath);
            Assert.IsNull(deviceObject);
        }

        [TestCase("System.IO.StreamWriter", Description = "Not usable for writing binary data.")]
        [TestCase("System.IO.BinaryWriter", Description = "Nothing is written. No exception is thrown.")]
        public void WriteFileWithRandomContent(String writerType) {
            DeviceManager deviceManager = new DeviceManager();
            var devices = deviceManager.GetAllDevices();
            var myDevice = devices.Where(x => x.Name.Equals(deviceName)).First();
            Stream stream = myDevice.OpenWrite(filePath, fileSize, true);
            Random random = new Random();
            using (IDisposable writer = (IDisposable)Activator.CreateInstance(Type.GetType(writerType), new object[] { stream })) {
                long count = 0;
                while (count < fileSize) {
                    byte[] buffer = new byte[4096];
                    random.NextBytes(buffer);
                    stream.Write(buffer, 0, buffer.Length);
                    count += buffer.Length;
                }
            }
            var deviceObject = myDevice.GetObjectFromPath(filePath);
            Assert.NotNull(deviceObject);
        }
    }
}