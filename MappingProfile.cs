using AutoMapper;
using myapi.DTOs;
using myapi.Model;  
namespace myapi
{
    public class MappingProfile : Profile
    {

        public MappingProfile() {

            // Employee -> EmployeeResponseDto
            CreateMap<Employee, EmployeeResponseDto>();

            // CreateEmployeeDto -> Employee
            CreateMap<CreateEmployeeDto, Employee>();

            // UpdateEmployeeDto -> Employee
            CreateMap<UpdateEmployeeDto, Employee>();
        }

    }
}
