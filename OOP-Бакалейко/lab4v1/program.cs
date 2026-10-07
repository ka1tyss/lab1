
using System;
using System.Collections.Generic;

namespace Lab4Variant1
{
    public class Employee
    {
        private string _name;
        private decimal _salary;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public decimal Salary
        {
            get => _salary;
            set => _salary = value >= 0 ? value : 0;
        }

        public Employee(string name, decimal salary)
        {
            _name = name;
            _salary = salary;
        }

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"[Employee] Ім'я: {Name}, Зарплата: {Salary:C}");
        }

        public string GetRole()
        {
            return "Звичайний працівник (Employee)";
        }
    }

    public class Manager : Employee
    {
        private string _department;

        public string Department
        {
            get => _department;
            set => _department = value;
        }

        public Manager(string name, decimal salary, string department)
            : base(name, salary)
        {
            _department = department;
        }

        public override void DisplayInfo()
        {
            Console.WriteLine($"[Manager] Ім'я: {Name}, Зарплата: {Salary:C}, Відділ: {Department}");
        }

        public void ManageTeam()
        {
            Console.WriteLine($"--> Менеджер {Name} організовує роботу відділу {Department}.");
        }

        public new string GetRole()
        {
            return "Менеджер відділу (Manager)";
        }
    }

    public class Director : Employee
    {
        private decimal _bonus;

        public decimal Bonus
        {
            get => _bonus;
            set => _bonus = value >= 0 ? value : 0;
        }

        public Director(string name, decimal salary, decimal bonus)
            : base(name, salary)
        {
            _bonus = bonus;
        }

        public override void DisplayInfo()
        {
            decimal totalIncome = Salary + Bonus;
            Console.WriteLine($"[Director] Ім'я: {Name}, Оклад: {Salary:C}, Бонус: {Bonus:C} (Загалом: {totalIncome:C})");
        }

        public void LeadCompany()
        {
            Console.WriteLine($"--> Директор {Name} проводить стратегічну нараду компанії.");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Employee emp = new Employee("Олексій", 20000);
            Manager mgr = new Manager("Ірина", 35000, "IT Розробка");
            Director dir = new Director("Микола", 60000, 15000);

            emp.DisplayInfo();
            
            mgr.DisplayInfo();
            mgr.ManageTeam();

            dir.DisplayInfo();
            dir.LeadCompany();

            List<Employee> staff = new List<Employee> { emp, mgr, dir };

            foreach (Employee e in staff)
            {
                e.DisplayInfo();
            }

            Console.WriteLine(mgr.GetRole());

            Employee mgrAsEmp = mgr;
            Console.WriteLine(mgrAsEmp.GetRole());
        }
    }
}