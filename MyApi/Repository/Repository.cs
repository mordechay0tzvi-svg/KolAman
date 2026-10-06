using System.ComponentModel.DataAnnotations;
using Dtos;
using Microsoft.EntityFrameworkCore;
using Models;
using DataContext;
namespace Repositories;
public class Repository : IRepository
{
    private readonly Context _db;
    public Repository (Context db)
    {
        _db = db;
    }
    public async Task<GenSumDto> GetGenSum()
    {
        return new GenSumDto
        {
            South = await _db.SouthAlerts.Select(a => a).CountAsync(),
            Notrh = await _db.NorthAlerts.Select(a => a).CountAsync(),
            Center = await  _db.CenterAlerts.Select(a => a).CountAsync(),
            Overseas = await  _db.OverseasAlerts.Select(a => a).CountAsync()
        };
    }
    public ByPriorityDto GetByPriority()
    {
        return new ByPriorityDto
        {
            South = _db.SouthAlerts.GroupBy(r => r.priority).Select(p => new {Priority = p.Key, Count = p.Count()}),
            Notrh = _db.NorthAlerts.GroupBy(r => r.priority).Select(p => new {Priority = p.Key, Count = p.Count()}),
            Center = _db.CenterAlerts.GroupBy(r => r.priority).Select(p => new {Priority = p.Key, Count = p.Count()}),
            Overseas = _db.OverseasAlerts.GroupBy(r => r.priority).Select(p => new {Priority = p.Key, Count = p.Count()})
        };
    }
    public ByStatusDto GetByStatus()
    {
        return new ByStatusDto
        {
            South = _db.SouthAlerts.GroupBy(r => r.status).Select(p => new {Status = p.Key, Count = p.Count()}),
            Notrh = _db.NorthAlerts.GroupBy(r => r.status).Select(p => new {Status = p.Key, Count = p.Count()}),
            Center = _db.CenterAlerts.GroupBy(r => r.status).Select(p => new {Status = p.Key, Count = p.Count()}),
            Overseas = _db.OverseasAlerts.GroupBy(r => r.status).Select(p => new {Status = p.Key, Count = p.Count()})
        };
    }
    public string GetHottestSector()
    {
        int lowPriorityScore = 1;
        int mediumPriorityScore = 2;
        int highPriorityScore = 3;
        int criticalPriorityScore = 5;

        var southScore = 0;
        southScore += _db.SouthAlerts.Where(a => a.priority == "LOW").Count() * lowPriorityScore;
        southScore += _db.SouthAlerts.Where(a => a.priority == "MEDIUM").Count() * mediumPriorityScore;
        southScore += _db.SouthAlerts.Where(a => a.priority == "HIGH").Count() * highPriorityScore;
        southScore += _db.SouthAlerts.Where(a => a.priority == "CRITICAL").Count() * criticalPriorityScore;
        
        double avgSouthAlertScore = southScore / _db.SouthAlerts.Select(a => a).Count();


        var northScore = 0;
        northScore += _db.NorthAlerts.Where(a => a.priority == "LOW").Count() * lowPriorityScore;
        northScore += _db.NorthAlerts.Where(a => a.priority == "MEDIUM").Count() * mediumPriorityScore;
        northScore += _db.NorthAlerts.Where(a => a.priority == "HIGH").Count() * highPriorityScore;
        northScore += _db.NorthAlerts.Where(a => a.priority == "CRITICAL").Count() * criticalPriorityScore;
        
        double avgNorthAlertScore = northScore / _db.NorthAlerts.Select(a => a).Count();

        var centerScore = 0;
        centerScore += _db.CenterAlerts.Where(a => a.priority == "LOW").Count() * lowPriorityScore;
        centerScore += _db.CenterAlerts.Where(a => a.priority == "MEDIUM").Count() * mediumPriorityScore;
        centerScore += _db.CenterAlerts.Where(a => a.priority == "HIGH").Count() * highPriorityScore;
        centerScore += _db.CenterAlerts.Where(a => a.priority == "CRITICAL").Count() * criticalPriorityScore;
        
        double avgCenterAlertScore = centerScore / _db.CenterAlerts.Select(a => a).Count();


        var overseasScore = 0;
        overseasScore += _db.SouthAlerts.Where(a => a.priority == "LOW").Count() * lowPriorityScore;
        overseasScore += _db.SouthAlerts.Where(a => a.priority == "MEDIUM").Count() * mediumPriorityScore;
        overseasScore += _db.SouthAlerts.Where(a => a.priority == "HIGH").Count() * highPriorityScore;
        overseasScore += _db.SouthAlerts.Where(a => a.priority == "CRITICAL").Count() * criticalPriorityScore;

        double avgOverseasAlertScore = overseasScore / _db.OverseasAlerts.Select(a => a).Count();

        Dictionary<string, double> allScores = new Dictionary<string, double>();
        allScores.Add("south", avgSouthAlertScore);
        allScores.Add("north", avgNorthAlertScore);
        allScores.Add("center", avgCenterAlertScore);
        allScores.Add("overseas", avgOverseasAlertScore);
          
        var hottestSectorScore = allScores.Max(a => a.Value);
        var hottestSectorName = allScores.FirstOrDefault(s => s.Value == hottestSectorScore);
        return hottestSectorName.Key;
    }

}
