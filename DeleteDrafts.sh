#!/bin/bash

# GitHub repository details (replace these with your repo owner and name)
REPO_OWNER="Fortunevale"
REPO_NAME="ProjectMakoto"

# Check if dry-run switch is passed
dry_run=false
if [[ "$1" == "--dry-run" ]]; then
  dry_run=true
fi

# List all draft releases associated with the repository
echo "Fetching all draft releases for repository $REPO_OWNER/$REPO_NAME..."

draft_releases=$(gh release list --repo "$REPO_OWNER/$REPO_NAME" --limit 100 | grep "Draft")

if [[ -z "$draft_releases" ]]; then
  echo "No draft releases found."
  exit 0
fi

# Iterate through each draft release
echo "$draft_releases" | while read -r release; do
  release_tag=$(echo "$release" | awk '{print $1}')
  echo "Found draft release: $release_tag"

  if [[ "$dry_run" == true ]]; then
    echo "[DRY RUN] Draft release '$release_tag' would be deleted."
  else
    echo "Deleting draft release '$release_tag'..."
    gh release delete "$release_tag" --repo "$REPO_OWNER/$REPO_NAME" --yes
    echo "Draft release '$release_tag' deleted."
  fi
done
