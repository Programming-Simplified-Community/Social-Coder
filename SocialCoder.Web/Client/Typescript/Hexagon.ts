interface HexagonOptions 
{
    /**
     * Classname to look for in the DOM
     */
    container: string;
    
    /**
     * Width of hexagon
     */
    width: number;

    /**
     * Height of hexagon
     */
    height: number;

    /**
     * Should there be rounded corners?
     */
    roundedCorners: boolean;

    /**
     * Radius for rounded corners
     */
    roundedCornerRadius: number;

    /**
     * Filled?
     */
    fill: boolean;

    /**
     * Line color for hexagon
     */
    lineColor: string;

    /**
     * Line width for hexagon
     */
    lineWidth: number;

    clip: boolean;
    
    gradient: GradientOptions;
    scale: ScaleOptions;
}

interface GradientOptions
{
    colors: string[];
}

interface ScaleOptions 
{
    start: number;
    end: number;
    stop: number;
}

/**
 * This function is tied to the xm_plugins.min.js file in the wwwroot/js folder
 * @param options
 */
function createHexagon(options: HexagonOptions)
{
    // Badger: This is excluded because it's included as an external library
    // @ts-ignore
    return new XM_Hexagon(options);
}