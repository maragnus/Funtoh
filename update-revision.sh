#!/bin/sh
# This script is used at build time within the CI/CD pipeline to update the Larp.Landing.Shared.Revision.BuildRevision const
echo "namespace Funtoh.Web;

public static class Revision
{
    public const string BuildRevision = \"$1\";
}" > "$(dirname "$0")/Funtoh.Web/Revision.cs"