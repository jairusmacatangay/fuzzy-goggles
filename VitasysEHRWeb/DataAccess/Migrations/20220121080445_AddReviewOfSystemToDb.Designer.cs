﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VitasysEHR.DataAccess;

#nullable disable

namespace VitasysEHR.DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220121080445_AddReviewOfSystemToDb")]
    partial class AddReviewOfSystemToDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("VitasysEHR.Models.Allergy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<bool>("IsAnesthesia")
                        .HasColumnType("bit");

                    b.Property<bool>("IsAntibiotics")
                        .HasColumnType("bit");

                    b.Property<bool>("IsAspirin")
                        .HasColumnType("bit");

                    b.Property<bool>("IsLatex")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSulfa")
                        .HasColumnType("bit");

                    b.Property<string>("Other")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Allergies");
                });

            modelBuilder.Entity("VitasysEHR.Models.AppointmentStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("AppointmentStatuses");
                });

            modelBuilder.Entity("VitasysEHR.Models.Clinic", b =>
                {
                    b.Property<int>("ClinicId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ClinicId"), 1L, 1);

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClinicName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<string>("EmailAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("bit");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("LogoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MobilePhone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OfficePhone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Province")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ZipCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ClinicId");

                    b.ToTable("Clinics");
                });

            modelBuilder.Entity("VitasysEHR.Models.Condition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Conditions");
                });

            modelBuilder.Entity("VitasysEHR.Models.FolderType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("FolderTypes");
                });

            modelBuilder.Entity("VitasysEHR.Models.OralCavity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClassType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAdvPerio")
                        .HasColumnType("bit");

                    b.Property<bool>("IsClenching")
                        .HasColumnType("bit");

                    b.Property<bool>("IsClicking")
                        .HasColumnType("bit");

                    b.Property<bool>("IsCrossbite")
                        .HasColumnType("bit");

                    b.Property<bool>("IsEarlyPerio")
                        .HasColumnType("bit");

                    b.Property<bool>("IsGingivitis")
                        .HasColumnType("bit");

                    b.Property<bool>("IsMidlineDeviation")
                        .HasColumnType("bit");

                    b.Property<bool>("IsModPerio")
                        .HasColumnType("bit");

                    b.Property<bool>("IsMusclePain")
                        .HasColumnType("bit");

                    b.Property<bool>("IsOverbite")
                        .HasColumnType("bit");

                    b.Property<bool>("IsOverjet")
                        .HasColumnType("bit");

                    b.Property<bool>("IsTrismus")
                        .HasColumnType("bit");

                    b.Property<string>("OrthoApplication")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("OralCavities");
                });

            modelBuilder.Entity("VitasysEHR.Models.Patient", b =>
                {
                    b.Property<int>("PatientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PatientId"), 1L, 1);

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DOB")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HomeNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("bit");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("datetime2");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MiddleName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MobileNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PortalAccess")
                        .HasColumnType("bit");

                    b.Property<bool>("PrivacyNotice")
                        .HasColumnType("bit");

                    b.Property<string>("ProfPicUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Province")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ZipCode")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PatientId");

                    b.ToTable("Patients");
                });

            modelBuilder.Entity("VitasysEHR.Models.PaymentMethod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Method")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("PaymentMethods");
                });

            modelBuilder.Entity("VitasysEHR.Models.PaymentStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("PaymentStatuses");
                });

            modelBuilder.Entity("VitasysEHR.Models.RestoProstho", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("RestoProsthos");
                });

            modelBuilder.Entity("VitasysEHR.Models.ReviewOfSystem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<bool>("IsAids")
                        .HasColumnType("bit");

                    b.Property<bool>("IsAnemia")
                        .HasColumnType("bit");

                    b.Property<bool>("IsAngina")
                        .HasColumnType("bit");

                    b.Property<bool>("IsArthritis")
                        .HasColumnType("bit");

                    b.Property<bool>("IsAsthma")
                        .HasColumnType("bit");

                    b.Property<bool>("IsBleeding")
                        .HasColumnType("bit");

                    b.Property<bool>("IsBloodDisease")
                        .HasColumnType("bit");

                    b.Property<bool>("IsCancer")
                        .HasColumnType("bit");

                    b.Property<bool>("IsChestPain")
                        .HasColumnType("bit");

                    b.Property<bool>("IsDiabetes")
                        .HasColumnType("bit");

                    b.Property<bool>("IsEmphysema")
                        .HasColumnType("bit");

                    b.Property<bool>("IsEpilepsy")
                        .HasColumnType("bit");

                    b.Property<bool>("IsFainting")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHayFever")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHbp")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHeadInjury")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHeartAttack")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHeartDisease")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHeartMurmur")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHeartSurgery")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHepa")
                        .HasColumnType("bit");

                    b.Property<bool>("IsJaundice")
                        .HasColumnType("bit");

                    b.Property<bool>("IsJointReplacement")
                        .HasColumnType("bit");

                    b.Property<bool>("IsKidney")
                        .HasColumnType("bit");

                    b.Property<bool>("IsLbp")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRadiation")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRespiratory")
                        .HasColumnType("bit");

                    b.Property<bool>("IsRheumatic")
                        .HasColumnType("bit");

                    b.Property<bool>("IsStd")
                        .HasColumnType("bit");

                    b.Property<bool>("IsStomach")
                        .HasColumnType("bit");

                    b.Property<bool>("IsStroke")
                        .HasColumnType("bit");

                    b.Property<bool>("IsSwollenAnkles")
                        .HasColumnType("bit");

                    b.Property<bool>("IsThyroid")
                        .HasColumnType("bit");

                    b.Property<bool>("IsTuberculosis")
                        .HasColumnType("bit");

                    b.Property<bool>("IsWeightLoss")
                        .HasColumnType("bit");

                    b.Property<string>("Other")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ReviewOfSystems");
                });

            modelBuilder.Entity("VitasysEHR.Models.Tooth", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("ToothNumber")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Teeth");
                });

            modelBuilder.Entity("VitasysEHR.Models.TreatmentType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("TreatmentTypes");
                });
#pragma warning restore 612, 618
        }
    }
}