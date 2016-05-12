<#
.SYNOPSIS 
A collection of test function to exercise the entities controller.

.DESCRIPTION
A collection of test function to exercise the entities controller.

.EXAMPLE
 #Load module
 ./Test-Functions.ps1

.EXAMPLE
# Pound the SF API Gateway with bunch of test random tansactions
Generate-EntityTransactions -baseUrl http://localhost:8146 -iterations 10

.EXAMPLE
# View the selected entity identfied by the type and the business key
View-Entity -baseUrl http://localhost:8146 -type 0 -$businessKey 0

.LINK
No links available or needed
#>

Function Generate-EntityTransactions($baseUrl = 'http://localhost:8146', $iterations = 20)
{
    Try {
        Write-Host "Generating $iterations random transactions against $baseUrl ...." -ForegroundColor Green

        $url = "$baseUrl/api/entities/transaction"

        foreach($i in 1..$iterations)
        {
            $soldItems = get-random -minimum 1 -maximum 30;
            $revenue = $soldItems * 75; # 75 = price per item
            $tax = ($revenue * 10) /100; # 10 = tax %
            $shipping = ($revenue * 5.5) /100; # 5.5 = shipping %
            $body = @{
                TransactionDate = Get-Date -format "yyy-mm-ddTHH:mm:ss";
                TransactionType = get-random -input 0, 0 -count 1;
                EntityType = 3;
                BusinessKey = get-random -input 30, 31, 130, 131, 141 -count 1;
                SoldItems = $soldItems;
                Revenue = $revenue;
                Tax = $tax;
                Shipping = $shipping
            }
            Write-Host "This is the JSON we are to post for iteration # $i...." -ForegroundColor yellow
            $json = ConvertTo-Json $body -Depth 3
            $json

	        $result = Invoke-RestMethod -Uri $url -Headers @{"Content-Type"="application/json" } -Body $json -Method POST -TimeoutSec 600
        }        
    } Catch {
        Write-Host "Failure message: $_.Exception.Message" -ForegroundColor red
        Write-Host "Failure stack trace: $_.Exception.StackTrace" -ForegroundColor red
        Write-Host "Failure inner exception: $_.Exception.InnerException" -ForegroundColor red
    }
}

Function View-Entity($baseUrl = 'http://localhost:8146', $type = 0, $businessKey = 0)
{
    Try {
        Write-Host "Requesting $type[$businessKey] entity view ...." -ForegroundColor Green

        $url = "$baseUrl/api/entities/$type/$businessKey"
	    $result = Invoke-RestMethod -Uri $url -Headers @{"Content-Type"="application/json" } -Method GET -TimeoutSec 600
        $json = ConvertTo-Json $result -Depth 3
        $json
    } Catch {
        Write-Host "Failure message: $_.Exception.Message" -ForegroundColor red
        Write-Host "Failure stack trace: $_.Exception.StackTrace" -ForegroundColor red
        Write-Host "Failure inner exception: $_.Exception.InnerException" -ForegroundColor red
    }
}

# Auto run the test for deployment tests
#Generate-EntityTransactions -baseUrl http://yanglesfabric2.eastus.cloudapp.azure.com:8080 -iterations 50
Generate-EntityTransactions -baseUrl http://localhost:8080 -iterations 2000

