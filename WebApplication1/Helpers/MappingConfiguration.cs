namespace DefaultHRManagementSystem.Helpers
{
    public class MappingConfiguration
    {
        public static void ConfigureMapster()
        {
            //TypeAdapterConfig<EmployeeDto, Employee>.NewConfig().Map(dest => dest.Department!.Name, src => src.DepartmentName);

            //TypeAdapterConfig<AttendanceDto, Attendance>.NewConfig().Map(dest => dest.Employee!.FullName, src => src.EmployeeName);

            //TypeAdapterConfig<SpecialLeaveDto, SpecialLeave>.NewConfig().Map(dest => dest.Employee!.FullName, src => src.EmployeeName);

            //TypeAdapterConfig<Employee, EmployeeDto>.NewConfig().Map(dest => dest.DepartmentName, src => src.Department!.Name);

            //TypeAdapterConfig<Attendance, AttendanceDto>.NewConfig().Map(dest => dest.EmployeeName, src => src.Employee!.FullName);

            //TypeAdapterConfig<SpecialLeave, SpecialLeaveDto>.NewConfig().Map(dest => dest.EmployeeName, src => src.Employee!.FullName);
        } 
    }
}
