using AutoMapper;
using Food.APIs.DTOs;
using Food.Domain.Models;
using Food.Domain.Repositories;
using Food.Domain.Specifications.DepartmentSpec;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    public class DepartmentController : APIBaseController
    {
        private readonly IGenericRepository<Department> departmentRepo;
        private readonly ILogger<DepartmentController> logger;
        private readonly IMapper mapper;

        public DepartmentController(IGenericRepository<Department> departmentRepo,ILogger<DepartmentController> logger,IMapper mapper)
        {
            this.departmentRepo = departmentRepo;
            this.logger = logger;
            this.mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartmentToReturnDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDepartments()
        {
            var Spec = new DepartmentSpec();
            var Departments = await departmentRepo.GetAllAsync(Spec);
            var DepartmentDtos = mapper.Map<IEnumerable<Department>, IEnumerable<DepartmentToReturnDto>>(Departments);
            return Ok(DepartmentDtos);
        }
    }
}
