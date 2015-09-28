# AzureBulkDeleteBlobs
Bulk delete Azure Blobs using any URL path and file name pattern and optimized with parallel threads


Arguments: ```<file name or extension pattern> <folder path> <container 1> <container 2> .. <container n> -verbose```
* **search pattern:** characters to be matched in a blob's URL path pattern. Examples:
  * *"/logs/"* matches files in any folder called "logs"
  * *".log"* matches files with extension ".log"
* **folder path:** Examples:
 * *"public"* Looks for files in this path
 * *"public\logs"* Looks for files in this path
* **container(s):** list of blob container name(s) to look in
* **verbose:** list URL of any blob that is found
