using System;

namespace lab6v1
{
    public class Employee
    {
        private string name;
        private decimal salary;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public decimal Salary
        {
            get => salary;
            set => salary = value >= 0 ? value : 0;
        }

        public Employee(string name, decimal salary)
        {
            Name = name;
            Salary = salary;
        }

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"[Employee] Ім'я: {Name}, Зарплата: {Salary:C}");
        }

        public string GetRole()
        {
            return "Працівник (Employee)";
        }
    }

    public class Manager : Employee
    {
        private string department;

        public string Department
        {
            get => department;
            set => department = value;
        }

        public Manager(string name, decimal salary, string department) 
            : base(name, salary)
        {
            Department = department;
        }

        public override void DisplayInfo()
        {
            Console.WriteLine($"[Manager] Ім'я: {Name}, Зарплата: {Salary:C}, Відділ: {Department}");
        }

        public void ManageTeam()
        {
            Console.WriteLine($"{Name} керує командою у відділі '{Department}'.");
        }

        public new string GetRole()
        {
            return "Менеджер (Manager)";
        }
    }

    public class Director : Manager
    {
        private decimal bonus;

        public decimal Bonus
        {
            get => bonus;
            set => bonus = value >= 0 ? value : 0;
        }

        public Director(string name, decimal salary, string department, decimal bonus) 
            : base(name, salary, department)
        {
            Bonus = bonus;
        }

        public override void DisplayInfo()
        {
            Console.WriteLine($"[Director] Ім'я: {Name}, Зарплата: {Salary:C}, Відділ: {Department}, Премія: {Bonus:C}");
        }

        public void LeadCompany()
        {
            Console.WriteLine($"{Name} керує всією компанією стратегічно.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Employee emp = new Employee("Олександр", 15000m);
            Manager mgr = new Manager("Марія", 28000m, "IT");
            Director dir = new Director("Іван Петрович", 55000m, "Виконавчий", 20000m);

            emp.DisplayInfo();
            mgr.DisplayInfo();
            dir.DisplayInfo();
            
            Console.WriteLine();
            Employee[] employees = new Employee[] { emp, mgr, dir };

            foreach (var e in employees)
            {
                e.DisplayInfo(); 
            }

            Console.WriteLine();
            mgr.ManageTeam();
            dir.LeadCompany();

            Console.WriteLine();
            Console.WriteLine($"Прямий виклик Manager.GetRole(): {mgr.GetRole()}");
            
            Employee refToManagerAsEmployee = mgr;
            Console.WriteLine($"Виклик GetRole() через посилання типу Employee: {refToManagerAsEmployee.GetRole()}");

            Console.WriteLine("\nПорівняння з поліморфним методом override:");
            refToManagerAsEmployee.DisplayInfo();
        }
    }
}