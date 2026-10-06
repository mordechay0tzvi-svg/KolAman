using System.ComponentModel.DataAnnotations;
using Dtos;
using Microsoft.EntityFrameworkCore;
using Models;
using DataContext;
namespace Repositories;
public interface IRepository
{
    Task<GenSumDto> GetGenSum();
    ByPriorityDto GetByPriority();
    ByStatusDto GetByStatus();
    string GetHottestSector();
    Task<IEnumerable<specificDayDto>> specificDay(int month, int day, string sector);
}
