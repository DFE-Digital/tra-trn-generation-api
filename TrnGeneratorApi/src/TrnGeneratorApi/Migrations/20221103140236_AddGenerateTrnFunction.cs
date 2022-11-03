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
        trn_info
    SET
        is_claimed = TRUE
    WHERE
        trn = (SELECT
                    trn
               FROM
                    trn_info
               WHERE
                    is_claimed IS FALSE
               ORDER BY
                    trn
               FOR UPDATE SKIP LOCKED
               LIMIT 1)
    RETURNING trn
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
