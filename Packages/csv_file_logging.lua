local Date = "2025/07/01"

-- create package table
local P = {}

luanet.load_assembly("Bruker.Lc")

---Log results in a csv file at Bruker proteoElute/MaintananceLogs in the corresponding procedure run directory
---@param context IProcedureExecutionContext
---@param fileName string Enter without .csv extention
---@param test string
---@param parameter string
---@param value number|string
---@param unit string
---@param decimalPlaces number
function P.logValueInCSVFile(context, fileName, test, parameter, value, unit, decimalPlaces)
    decimalPlaces = decimalPlaces or -1
    local date = os.date("%x %X")
    local path = context.LogFileDataPath
    if path == nil then
        context:Log("Can't write result to csv file because log file data path is empty.")
        return
    end
    local fileLocation = path.."\\results.csv"
    ---@type file*?
    local file = nil

    
    local function round(num, numDecimalPlaces)
        local mult = 10^numDecimalPlaces
        return math.floor(num * mult + 0.5) / mult
    end
    local function file_exists()
        local f = io.open(fileLocation)
        local fileExist = f ~= nil and io.close(f)
        return fileExist
        end
    local function writeStringToFile()
        local writeHeader = file_exists() == false
        file = io.open(fileLocation, "a")		-- Opens a file in append mode
        if file ~= nil then
            io.output(file)
            context:Sleep(100)
            io.output(file)
            context:Sleep(100)
            if writeHeader then
                local header = "Date Time"..";".."Test"..";".."Name"..";".."Value"..";".."Unit".."\n"
                io.write(header)
                context:Sleep(100)
            end
            local strg
            if type(value) == "number" then
                if decimalPlaces > -1 then
                    value = round(value, decimalPlaces)
                else
                    value = round(value, 5)
                end
                strg = date..";"..test..";"..parameter..";"..tostring(value)..";"..unit.."\n"
            else 
                strg = date..";"..test..";"..parameter..";"..value..";"..unit.."\n"
            end
            io.write(strg)
            context:Sleep(100)
            io.close(file)
        end
    end

    context:Log("Logging data in file {0}", fileLocation)
    pcall(writeStringToFile)
end
return P