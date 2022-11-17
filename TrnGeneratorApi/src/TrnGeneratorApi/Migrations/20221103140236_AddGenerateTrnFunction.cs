using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrnGeneratorApi.Migrations;

public partial class AddGenerateTrnFunction : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var functionSql = @"
CREATE OR REPLACE FUNCTION fn_generate_trn() 
RETURNS INT
LANGUAGE plpgsql
AS
$$
DECLARE
    next_available_trn INT;
BEGIN
    UPDATE
        trn_range
    SET
        is_exhausted = CASE WHEN next_trn >= to_trn THEN TRUE ELSE FALSE END, 
        next_trn = next_trn + 1
    WHERE
        from_trn = (SELECT
                        from_trn
                    FROM
                        trn_range
                    WHERE
                        is_exhausted IS FALSE
                    ORDER BY
                        from_trn
                    FOR UPDATE
                    LIMIT 1)
    RETURNING next_trn - 1
    INTO next_available_trn;
    
    RETURN next_available_trn;
END;
$$
";

        migrationBuilder.Sql(functionSql);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FUNCTION fn_generate_trn()");
    }
}
