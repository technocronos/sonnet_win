using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.IO;

public class LoggingDownloadHandler : DownloadHandlerScript
{
    MemoryStream buffer = null;
    string json;

    public string getJson()
    {
        return json;
    }

    // Standard scripted download handler - will allocate memory on each ReceiveData callback
    public LoggingDownloadHandler() : base()
    {
    }

    // Pre-allocated scripted download handler
    // Will reuse the supplied byte array to deliver data.
    // Eliminates memory allocation.
    public LoggingDownloadHandler(byte[] buffer) : base(buffer)
    {
    }

    // Required by DownloadHandler base class. Called when you address the 'bytes' property.
    protected override byte[] GetData() { return null; }

    // Called once per frame when data has been received from the network.
    protected override bool ReceiveData(byte[] data, int dataLength)
    {
        if (data == null || data.Length < 1)
        {
            //Debug.Log("LoggingDownloadHandler :: ReceiveData - received a null/empty buffer");
            return false;
        }

        //Debug.Log(string.Format("LoggingDownloadHandler :: ReceiveData - received {0} bytes", dataLength));

        this.buffer.Write(data, 0, dataLength);
        return true;
    }

    // Called when all data has been received from the server and delivered via ReceiveData
    protected override void CompleteContent()
    {
        //Debug.Log("LoggingDownloadHandler :: CompleteContent - DOWNLOAD COMPLETE!");

        base.CompleteContent();
        this.json = System.Text.Encoding.UTF8.GetString(this.buffer.ToArray());

        this.buffer.Dispose();
    }

    // Called when a Content-Length header is received from the server.
    protected override void ReceiveContentLength(int contentLength)
    {
        //Debug.Log(string.Format("LoggingDownloadHandler :: ReceiveContentLength - length {0}", contentLength));

        base.ReceiveContentLength(contentLength);
        this.buffer = new MemoryStream(contentLength);
    }
}
