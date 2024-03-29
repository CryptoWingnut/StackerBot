﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StackerBot;

#nullable disable

namespace StackerBot.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240211210949_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StackerBot.Models.YouTubeSubscriptionModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("AddedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("added_at");

                    b.Property<decimal>("AddedBy")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("added_by");

                    b.Property<string>("ChannelId")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("channel_id");

                    b.Property<string>("ChannelName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("channel_name");

                    b.HasKey("Id");

                    b.ToTable("youtube_subscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
