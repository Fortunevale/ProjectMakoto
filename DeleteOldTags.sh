#!/bin/bash

# Check if dry-run switch is passed
dry_run=false
if [[ "$1" == "--dry-run" ]]; then
  dry_run=true
fi

# Get current date in seconds since the epoch
current_date=$(date +%s)

# Find tags older than a month (30 days)
month_in_seconds=$((30 * 24 * 60 * 60))

# Get the list of tags
tags=$(git tag)

echo "Checking for tags older than one month..."

for tag in $tags; do
  # Get the date of the tag in seconds since the epoch
  tag_date=$(git log -1 --format=%at "$tag")

  # Calculate the age of the tag
  tag_age=$((current_date - tag_date))

  # Check if the tag is older than a month
  if [[ $tag_age -ge $month_in_seconds ]]; then
    if [[ "$dry_run" == true ]]; then
      echo "[DRY RUN] Tag '$tag' would be deleted (Created: $(date -d @$tag_date))"
    else
      echo "Deleting tag '$tag' (Created: $(date -d @$tag_date))"
      git tag -d "$tag"
      git push --delete origin "$tag"
    fi
  fi
done
