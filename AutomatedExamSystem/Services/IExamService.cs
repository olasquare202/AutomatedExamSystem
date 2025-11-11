using AutomatedExamSystem.Models;

namespace AutomatedExamSystem.Services
{
    public interface IExamService
    {
        Task<bool> StartAttemptAsync(Candidate candidate);
        //Task StartAttemptAsync(Candidate candidate);
        //Task SubmitAttemptAsync(int candidateId, Dictionary<int, char> answers);
        Task<CandidateAttempt> SubmitAttemptAsync(int candidateId, Dictionary<int, char> answers);
        
        Task<bool> IsRegistrationOpenAsync(IConfiguration config);
        Task<bool> IsTestWindowOpenAsync(IConfiguration config);
    }
}
