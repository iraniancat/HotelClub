// src/HotelReservation.Domain/Constants/RoleConstants.cs
using System;

namespace HotelReservation.Domain.Constants;

public static class RoleConstants
{
    // Guids should be generated once and kept constant
    public static readonly Guid SuperAdminRoleId = Guid.Parse("c76ef8b1-9e87-42d3-a1f9-9b99fd9b904f"); // مثال، Guid واقعی تولید کنید
    public static readonly Guid ProvinceUserRoleId = Guid.Parse("B0E2C45A-B935-4C7A-9A9A-4A5B6C7D8E0A"); // مثال
    public static readonly Guid HotelUserRoleId = Guid.Parse("C0E2C45A-B935-4C7A-9A9A-4A5B6C7D8E1B");    // مثال
    public static readonly Guid EmployeeRoleId = Guid.Parse("D0E2C45A-B935-4C7A-9A9A-4A5B6C7D8E2C");     // مثال

    public const string SuperAdminRoleName = "SuperAdmin";
    public const string ProvinceUserRoleName = "ProvinceUser";
    public const string HotelUserRoleName = "HotelUser";
    public const string EmployeeRoleName = "Employee";
}