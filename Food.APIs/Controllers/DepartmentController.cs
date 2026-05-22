using AutoMapper;
using Food.APIs.DTOs;
using Food.Domain;
using Food.Domain.Models;
using Food.Domain.Specifications.DepartmentSpec;
using Microsoft.AspNetCore.Mvc;

namespace Food.APIs.Controllers
{
    public class DepartmentController : APIBaseController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<DepartmentController> logger;
        private readonly IMapper mapper;

        public DepartmentController(IUnitOfWork unitOfWork, ILogger<DepartmentController> logger,IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartmentToReturnDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDepartments()
        {
            var Spec = new DepartmentSpec();
            var Departments = await unitOfWork.Repository<Department>().GetAllAsync(Spec);
            var DepartmentDtos = mapper.Map<IEnumerable<Department>, IEnumerable<DepartmentToReturnDto>>(Departments);
            return Ok(DepartmentDtos);
        }
    }
}
