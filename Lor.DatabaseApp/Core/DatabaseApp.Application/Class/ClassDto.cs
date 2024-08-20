﻿namespace DatabaseApp.Application.Class;

public record ClassDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required DateOnly Date { get; set; }
    public int GroupId { get; set; }
}