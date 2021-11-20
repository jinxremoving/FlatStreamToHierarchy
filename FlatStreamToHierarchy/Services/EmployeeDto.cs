using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace FlatStreamToHierarchy.Services
{
    public class EmployeeDto : IEquatable<EmployeeDto>
    {
        private string status;
        private readonly ReplaySubject<string> statusSubject = new ReplaySubject<string>(1);

        public EmployeeDto(int id, string name, int boss, string status)
        {
            Id = id;
            Name = name;
            BossId = boss;
            Status = status;
        }

        public int Id { get; }
        public int BossId { get; }
        public string Name { get; }
        public string Status
        {
            get => this.status;
            set
            {
                if (this.status != value)
                {
                    this.status = value;
                    this.statusSubject.OnNext(value);
                }
            }
        }

        public IObservable<string> StatusChanged => this.statusSubject.AsObservable();

        #region Equality Members

        public bool Equals(EmployeeDto other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EmployeeDto) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(EmployeeDto left, EmployeeDto right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EmployeeDto left, EmployeeDto right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
