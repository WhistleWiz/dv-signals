using System.Collections.Generic;

namespace Signals.Unity.Validation
{
    internal enum Status
    {
        Pass,
        Skip,
        Warning,
        Failure,
        Critical
    }

    internal class Result
    {
        private readonly List<ResultEntry> _warnings;

        public string Name;
        public Status Status {  get; private set; }
        public List<ResultEntry> Warnings => _warnings;
        public List<ResultEntry> Entries => _warnings.Count > 0 ? _warnings : new List<ResultEntry>() { new ResultEntry(Status, string.Empty)};

        public Result(string name)
        {
            Name = name;
            Status = Status.Pass;
            _warnings = new List<ResultEntry>();
        }

        private void Escalate(Status latest)
        {
            if (latest > Status)
            {
                Status = latest;
            }
        }

        public void AddWarning(string message)
        {
            Warnings.Add(new ResultEntry(Status.Warning, message));
            Escalate(Status.Warning);
        }

        public void AddFailure(string message)
        {
            Warnings.Add(new ResultEntry(Status.Failure, message));
            Escalate(Status.Failure);
        }

        public void AddCritical(string message)
        {
            Warnings.Add(new ResultEntry(Status.Critical, message));
            Escalate(Status.Critical);
        }

        public void Merge(Result other)
        {
            Warnings.AddRange(other.Warnings);
            Escalate(other.Status);
        }

        public static Result Skip(string name)
        {
            return new Result(name) { Status = Status.Skip };
        }
    }
}
