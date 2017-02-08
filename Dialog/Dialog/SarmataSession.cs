using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dialog
{
    class SarmataSession
    {
        Grpc.Core.Channel asrChannel = null;
        Sarmata.ASR.ASRClient asrClient = null;

        Grpc.Core.IClientStreamWriter<Sarmata.RecognizeRequest> requestStream = null;
        Grpc.Core.IAsyncStreamReader<Sarmata.RecognizeResponse> responseStream = null;

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public SarmataSession(String serviceAddress, Dictionary<String, String> settings)
        {
            asrChannel = new Grpc.Core.Channel(serviceAddress, Grpc.Core.ChannelCredentials.Insecure);
            asrClient = new Sarmata.ASR.ASRClient(asrChannel);
            
            var openStream = asrClient.Recognize(cancellationToken: new CancellationToken());
            requestStream = openStream.RequestStream;
            responseStream = openStream.ResponseStream;


            Sarmata.RecognizeRequest initialMessage = new Sarmata.RecognizeRequest()
            {
                InitialRequest = settings2request(settings)
            };

            requestStream.WriteAsync(initialMessage).Wait();
        }

        public void AddSamples(List<short> data)
        {
            Sarmata.RecognizeRequest audioRequest = new Sarmata.RecognizeRequest();
            audioRequest.AudioRequest = new Sarmata.AudioRequest();
            audioRequest.AudioRequest.EndOfStream = false;

            List<byte> samples_bytes = new List<byte>(2 * data.Count);
            foreach (var sample in data)
            {
                samples_bytes.AddRange(BitConverter.GetBytes(sample));
            }
            audioRequest.AudioRequest.Content = Google.Protobuf.ByteString.CopyFrom(samples_bytes.ToArray());
            requestStream.WriteAsync(audioRequest).Wait();
        }

        public Sarmata.RecognizeResponse WaitForResponse()
        {
            responseStream.MoveNext().Wait();
            var res = responseStream.Current;
            return res;
        }

        private Sarmata.InitialRecognizeRequest settings2request(Dictionary<String, String> settings)
        {
            var request = new Sarmata.InitialRecognizeRequest();

            foreach (var kv in settings)
            {
                Sarmata.ConfigField sessionIdField = new Sarmata.ConfigField
                {
                    Key = kv.Key,
                    Value = kv.Value
                };
                request.Config.Add(sessionIdField);
            }

            return request;
        }
    }
}
