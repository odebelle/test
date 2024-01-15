using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BusRepositoryApi;

[DbContext(typeof(BusRemoteOperatorContext))]
partial class BusRemoteOperatorContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("Shared.Models.Consumer", b =>
        {
            b.Property<int>("DispatchId")
                .HasColumnType("integer")
                .HasColumnName("dispatch_id");

            b.Property<DateTime?>("FirstRun")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("first_run");

            b.Property<string>("Information")
                .HasColumnType("text")
                .HasColumnName("information");

            b.Property<DateTime?>("LastError")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("last_error");

            b.Property<DateTime?>("LastRun")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("last_run");

            b.Property<DateTime?>("LastSuccess")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("last_success");

            b.Property<string>("Marshal")
                .HasColumnType("text")
                .HasColumnName("marshal");

            b.Property<string>("Topic")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("topic");

            b.HasKey("DispatchId")
                .HasName("pk_consumer");

            b.ToTable("consumer", (string)null);
        });

        modelBuilder.Entity("Shared.Models.Dispatch", b =>
        {
            b.Property<int>("DispatchId")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("dispatch_id");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("DispatchId"));

            b.Property<string>("Cluster")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("cluster");

            b.Property<bool>("Enabled")
                .HasColumnType("boolean")
                .HasColumnName("enabled");

            b.Property<bool?>("IsWorker")
                .HasColumnType("boolean")
                .HasColumnName("is_worker");

            b.Property<string>("Name")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("name");

            b.Property<string>("Subject")
                .HasColumnType("text")
                .HasColumnName("subject");

            b.HasKey("DispatchId")
                .HasName("pk_dispatch");

            b.ToTable("dispatch", (string)null);
        });

        modelBuilder.Entity("Shared.Models.Producer", b =>
        {
            b.Property<int>("DispatchId")
                .HasColumnType("integer")
                .HasColumnName("dispatch_id");

            b.Property<string>("Cron")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("cron");

            b.Property<DateTime?>("FirstRun")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("first_run");

            b.Property<string>("Information")
                .HasColumnType("text")
                .HasColumnName("information");

            b.Property<DateTime?>("LastError")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("last_error");

            b.Property<DateTime?>("LastRun")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("last_run");

            b.Property<DateTime?>("LastSuccess")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("last_success");

            b.Property<DateTime?>("NextRun")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("next_run");

            b.Property<string>("Topic")
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("topic");

            b.HasKey("DispatchId")
                .HasName("pk_producer");

            b.ToTable("producer", (string)null);
        });

        modelBuilder.Entity("Shared.Models.Consumer", b =>
        {
            b.HasOne("Shared.Models.Dispatch", "Dispatch")
                .WithOne("Consumer")
                .HasForeignKey("Shared.Models.Consumer", "DispatchId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                .HasConstraintName("fk_consumer_dispatch_dispatch_id");

            b.Navigation("Dispatch");
        });

        modelBuilder.Entity("Shared.Models.Producer", b =>
        {
            b.HasOne("Shared.Models.Dispatch", "Dispatch")
                .WithOne("Producer")
                .HasForeignKey("Shared.Models.Producer", "DispatchId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                .HasConstraintName("fk_producer_dispatch_dispatch_id");

            b.Navigation("Dispatch");
        });

        modelBuilder.Entity("Shared.Models.Dispatch", b =>
        {
            b.Navigation("Consumer");

            b.Navigation("Producer");
        });
#pragma warning restore 612, 618
    }
}