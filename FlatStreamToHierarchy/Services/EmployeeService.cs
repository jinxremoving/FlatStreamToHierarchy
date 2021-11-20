using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Kernel;

namespace FlatStreamToHierarchy.Services
{
    public class EmployeeService
    {
        private readonly SourceCache<EmployeeDto, int> _employees = new SourceCache<EmployeeDto, int>(x => x.Id);
        private readonly IDisposable _disposeMe;

        public EmployeeService()
        {
            _employees = new SourceCache<EmployeeDto, int>(emp => emp.Id);
            _employees.AddOrUpdate(CreateEmployees(25000));

            Random r = new Random();
            TimeSpan RandomTime() => TimeSpan.FromMilliseconds(r.Next(2500, 5000));
            var statusUpdater = TaskPoolScheduler.Default.ScheduleRecurringAction(RandomTime, () =>
            {
                _employees.Edit(cache =>
                {
                    foreach (var item in cache.Items)
                    {
                        item.Status = $"Status of {item.Name} at {DateTimeOffset.Now.Ticks}";
                    }
                });
            });

            _disposeMe = Disposable.Create(() =>
            {
                statusUpdater.Dispose();
            });

            this.Employees = _employees.AsObservableCache();
        }

        //public IObservableCache<EmployeeDto, int> Employees => _employees.AsObservableCache();
        public IObservableCache<EmployeeDto, int> Employees { get; }

        public void Promote(EmployeeDto promtedDto, int newBoss)
        {
            //in the real world, go to service then update the cache

            //update the cache with the emploee, 
            _employees.AddOrUpdate(new EmployeeDto(promtedDto.Id, promtedDto.Name, newBoss, promtedDto.Status));
        }


        public void Sack(EmployeeDto sackEmp)
        {
            //in the real world, go to service then updated the cache

            _employees.Edit(updater =>
            {
                //assign new boss to the workers of the sacked employee
                var workersWithNewBoss = updater.Items
                                    .Where(emp => emp.BossId == sackEmp.Id)
                                    .Select(dto => new EmployeeDto(dto.Id, dto.Name, sackEmp.BossId, dto.Status))
                                    .ToArray();

                updater.AddOrUpdate(workersWithNewBoss);

                //get rid of the existing person
                updater.Remove(sackEmp.Id);
            });


        }

        private IEnumerable<EmployeeDto> CreateEmployees(int numberToLoad)
        {
            var random = new Random();

            return Enumerable.Range(1, numberToLoad)
                .Select(i =>
                {
                    var boss = i % 1000 == 0 ? 0 : random.Next(0, i);
                    return new EmployeeDto(i, $"Person {i}", boss, "Nominal");
                });
        }
    }
}
