import type { BuildingType } from './types'

interface BuildingInfo {
  name: string
  description: string
}

export const buildingInfo: Record<BuildingType, BuildingInfo> = {
  MetalMine: { name: 'Metal Mine', description: 'Extracts metal, the backbone of every structure and hull.' },
  CrystalMine: { name: 'Crystal Mine', description: 'Grows crystal for electronics and alloys.' },
  DeuteriumSynthesizer: {
    name: 'Deuterium Synthesizer',
    description: "Filters deuterium from the planet's water. Works better on cold planets.",
  },
  SolarPlant: { name: 'Solar Plant', description: 'Turns starlight into the energy your mines run on.' },
  RoboticsFactory: { name: 'Robotics Factory', description: 'Construction drones that make every building go up faster.' },
  MetalStorage: { name: 'Metal Storage', description: 'Raises how much metal the planet can hold.' },
  CrystalStorage: { name: 'Crystal Storage', description: 'Raises how much crystal the planet can hold.' },
  DeuteriumTank: { name: 'Deuterium Tank', description: 'Raises how much deuterium the planet can hold.' },
}

export const buildingGroups: { title: string; buildings: BuildingType[] }[] = [
  { title: 'Resources', buildings: ['MetalMine', 'CrystalMine', 'DeuteriumSynthesizer', 'SolarPlant'] },
  { title: 'Facilities', buildings: ['RoboticsFactory'] },
  { title: 'Storage', buildings: ['MetalStorage', 'CrystalStorage', 'DeuteriumTank'] },
]
