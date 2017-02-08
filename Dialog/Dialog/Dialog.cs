using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dialog
{
    class Dialog
    {
        protected SarmataSession activeSession = null;

        protected String host;
        public Dialog()
        { }

        public void SetAddress(string hostAddress)
        {
            host = hostAddress;
        }


        public void Start(Action<string> print)
        {
            new Thread(() =>
            {
                Run(print);
            }).Start();
        }

        public virtual void Run(Action<string> print) { }

        public void AddSamples(List<short> data)
        {
            if (activeSession != null)
            {
                activeSession.AddSamples(data);
            }
        }

        public String status2string(Sarmata.RecognizeResponse response)
        {
            if (response.Status == Sarmata.ResponseStatus.RecognizerError ||
                response.Status == Sarmata.ResponseStatus.SemanticsFailure ||
                response.Status == Sarmata.ResponseStatus.GrammarLoadFailure ||
                response.Status == Sarmata.ResponseStatus.GrammarCompilationFailure)
            {
                return $"{response.Status}: {response.Error}";
            }
            if (response.Status == Sarmata.ResponseStatus.Success ||
                response.Status == Sarmata.ResponseStatus.PartialMatch)
            {
                String res = $"{response.Status}: ";
                foreach (var word in response.Results[0].Words)
                {
                    res += word.Transcript + " ";
                }
                if (response.Results[0].SemanticInterpretation != "")
                { 
                    res += $"[{response.Results[0].SemanticInterpretation}]";
                }
                return res;
            }
            return response.Status.ToString();
        }
    }
}
