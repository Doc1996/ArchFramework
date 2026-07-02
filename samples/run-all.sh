#!/usr/bin/env bash
set -e

cleanup() {
	kill 0 2>/dev/null || true
}

trap cleanup SIGINT SIGTERM EXIT

dotnet run --project samples/Leva.Framework.Sample.StateCounter --urls http://localhost:5101 &
dotnet run --project samples/Leva.Framework.Sample.StorageDesk --urls http://localhost:5102 &
dotnet run --project samples/Leva.Framework.Sample.LiveDashboard --urls http://localhost:5103 &
dotnet run --project samples/Leva.Framework.Sample.LocalIdentity --urls http://localhost:5104 &
dotnet run --project samples/Leva.Framework.Sample.WebIdentity --urls http://localhost:5105 &

echo "StateCounter:  http://localhost:5101"
echo "StorageDesk:   http://localhost:5102"
echo "LiveDashboard: http://localhost:5103"
echo "LocalIdentity: http://localhost:5104"
echo "WebIdentity:   http://localhost:5105"
echo
echo "Press Ctrl+C to stop all samples."

wait
