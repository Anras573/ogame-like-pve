import type { components } from '../api/schema'

type Schemas = components['schemas']

export type Planet = Schemas['PlanetDto']
export type PlanetSummary = Schemas['PlanetSummaryDto']
export type Resources = Schemas['ResourcesDto']
export type BuildingType = Schemas['BuildingType']
export type Building = Schemas['BuildingDto']
export type MissingRequirements = Schemas['MissingRequirementsDto']
export type UnitType = Schemas['UnitType']
export type Unit = Schemas['UnitDto']
export type ShipyardOrder = Schemas['ShipyardOrderDto']
export type ResearchType = Schemas['ResearchType']
export type ResearchOverview = Schemas['ResearchOverviewDto']
export type Technology = Schemas['ResearchDto']
