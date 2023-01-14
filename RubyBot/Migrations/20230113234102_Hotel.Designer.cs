﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RubyBot.Model;

#nullable disable

namespace RubyBot.Migrations
{
    [DbContext(typeof(RolePlayContext))]
    [Migration("20230113234102_Hotel")]
    partial class Hotel
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("RubyBot.Model.Hotel", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Size")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Hotels");
                });

            modelBuilder.Entity("RubyBot.Model.Persona", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Agility")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Locked")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte>("Parade")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Precision")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Resistance")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Strength")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("Personas");
                });

            modelBuilder.Entity("RubyBot.Model.Player", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("LastUsedId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Seed")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("LastUsedId")
                        .IsUnique();

                    b.ToTable("Players");
                });

            modelBuilder.Entity("RubyBot.Model.Room", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Busy")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("HotelId")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Number")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("HotelId");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("RubyBot.Model.Persona", b =>
                {
                    b.HasOne("RubyBot.Model.Player", "Player")
                        .WithMany("Personas")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("RubyBot.Model.Player", b =>
                {
                    b.HasOne("RubyBot.Model.Persona", "LastUsed")
                        .WithOne()
                        .HasForeignKey("RubyBot.Model.Player", "LastUsedId");

                    b.Navigation("LastUsed");
                });

            modelBuilder.Entity("RubyBot.Model.Room", b =>
                {
                    b.HasOne("RubyBot.Model.Hotel", "Hotel")
                        .WithMany("Rooms")
                        .HasForeignKey("HotelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Hotel");
                });

            modelBuilder.Entity("RubyBot.Model.Hotel", b =>
                {
                    b.Navigation("Rooms");
                });

            modelBuilder.Entity("RubyBot.Model.Player", b =>
                {
                    b.Navigation("Personas");
                });
#pragma warning restore 612, 618
        }
    }
}
