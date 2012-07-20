namespace OwinHelpers
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using BodyDelegate = System.Func<System.IO.Stream,System.Threading.CancellationToken,System.Threading.Tasks.Task>;

    internal class FileBody
    {
        private readonly FileInfo _fileInfo;

        public FileBody(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public BodyDelegate ToAction()
        {
            return (stream, token) =>
                {
                    using (var fileStream = _fileInfo.OpenRead())
                    {
                        fileStream.CopyTo(stream);
                    }
                    return TaskHelper.Completed();
                };
        }
    }
}
