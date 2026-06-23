using System;

namespace SBAR.Interaction
{
    public interface IVoiceInput
    {
        event Action<int> ChoiceRecognized;
        void StartListening(string[] options);
        void StopListening();
    }
}
