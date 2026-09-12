using System;
using System.Collections.Generic;
using System.Text;
using SmartPantry.Domain;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Domain.Products
{
    public class Product : AggregateRoot<int>
    {
    public string Name { get; set; }


        }   
}