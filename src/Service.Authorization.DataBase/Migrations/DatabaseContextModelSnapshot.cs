﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Service.Authorization.DataBase;

namespace Service.Authorization.DataBase.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("authorization")
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.5")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("Service.Authorization.DataBase.KillSessionAuditEntity", b =>
                {
                    b.Property<string>("SessionRootId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Ip")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("KillTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Reason")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("SessionId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("UserAgent")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.HasKey("SessionRootId");

                    b.ToTable("kills");
                });

            modelBuilder.Entity("Service.Authorization.DataBase.SessionAuditEntity", b =>
                {
                    b.Property<string>("SessionId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("BaseSessionId")
                        .HasColumnType("text");

                    b.Property<string>("BrandId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("BrokerId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ClientId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Ip")
                        .HasColumnType("text");

                    b.Property<string>("SessionRootId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("UserAgent")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<string>("WalletId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("SessionId");

                    b.ToTable("sessions");
                });
#pragma warning restore 612, 618
        }
    }
}
