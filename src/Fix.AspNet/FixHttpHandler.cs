namespace Fix.AspNet
{
    using System;
    using System.Web;

    public class FixHttpHandler : IHttpAsyncHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public bool IsReusable { get; private set; }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            var task = Bridge.RunContext(context);
            task.ContinueWith(t => cb(task));
            return task;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
        }
    }
}