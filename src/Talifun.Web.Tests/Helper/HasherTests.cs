using System;
using System.IO;
using System.Text;
using Rhino.Mocks;

namespace Talifun.Web.Tests.Helper
{
    public abstract class HasherTests : BehaviourTest<Hasher>
    {
        protected IRetryableFileOpener RetryableFileOpener = MockRepository.GenerateMock<IRetryableFileOpener>();

        protected override Hasher CreateSystemUnderTest()
        {
            return new Hasher(RetryableFileOpener);
        }
    }

    public abstract class WhenCalculatingMd5Hash : HasherTests
    {
        protected string TextToHash = "This is a test";
        protected string Md5HashOfTextToHash = "zhFORQHS9OLc6j4XtUbzOQ=="; //Hash value of bytes

        protected byte[] ByteArray;
        protected MemoryStream Stream;

        protected override void Given()
        {
            ByteArray = Encoding.ASCII.GetBytes(TextToHash);
            Stream = new MemoryStream(ByteArray);
        }

        protected string Hash;

        [Then]
        public void HashShouldMatch()
        {
            Hash.ShouldEqual(Md5HashOfTextToHash);
        }
    }

    public class WhenCalculatingMd5HashForEntireStream : WhenCalculatingMd5Hash
    {
        protected override void When()
        {
            Hash = SystemUnderTest.CalculateMd5Etag(Stream);
        }
    }

    public class WhenCalculatingMd5HashForPartOfStream : WhenCalculatingMd5Hash
    {
        protected int StartPosition;
        protected int EndPosition;

        protected override void Given()
        {
            StartPosition = 0;
            EndPosition = TextToHash.Length;

            base.Given();
        }

        protected override void When()
        {
            Hash = SystemUnderTest.CalculateMd5Etag(Stream, StartPosition, EndPosition);
        }
    }

    public class WhenCalculatingMd5HashForFile : WhenCalculatingMd5Hash
    {
        protected string FileName = "test.txt";
        protected FileInfo FileToHash;
        protected FileStream FileStream;

        protected override void Given()
        {
            base.Given();

            FileToHash = new FileInfo(FileName);
            FileStream = MockRepository.GenerateMock<FileStream>();

            RetryableFileOpener.Expect(x => x.OpenFileStream(FileToHash, 5, FileMode.Open, FileAccess.Read, FileShare.Read)).Return(FileStream);

            Func<byte[], int, int, int> read = Stream.Read;
            FileStream.Stub(x => x.Read(null, 0, 0)).IgnoreArguments().Do(read);
        }

        protected override void When()
        {
            Hash = SystemUnderTest.CalculateMd5Etag(FileToHash);
        }
    }
}